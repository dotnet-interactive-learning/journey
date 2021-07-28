using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
using Microsoft.DotNet.Interactive.Parsing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Extension
{
    public static class KernelExtensions
    {
        private static string _modelAnswerCommandName = "#!model-answer";

        public static CompositeKernel UseModelAnswerValidation(this CompositeKernel kernel)
        {
            var modelAnswerCommand = new Command(_modelAnswerCommandName);
            kernel.AddDirective(modelAnswerCommand);
            return kernel;
        }

        public static CompositeKernel UseProgressiveLearning(this CompositeKernel kernel, Lesson lesson)
        {
            kernel.UseProgressiveLearningMiddleware(lesson);
            return kernel;
        }

        public static CompositeKernel UseProgressiveLearning(this CompositeKernel kernel)
        {
            Option<Uri> fromUrlOption = new Option<Uri>(
                "--from-url",
                "Specify lesson source URL");

            Option<FileInfo> fromFileOption = new Option<FileInfo>(
                "--from-file",
                description: "Specify lesson source file",
                parseArgument: result =>
                {
                    var filePath = result.Tokens.Single().Value;
                    var fromUrlResult = result.FindResultFor(fromUrlOption);

                    if (fromUrlResult is not null)
                    {
                        result.ErrorMessage = $"The {fromUrlResult.Token.Value} and {(result.Parent as OptionResult).Token.Value} options cannot be used together";
                        return null;
                    }

                    else if (!File.Exists(filePath))
                    {
                        result.ErrorMessage = Resources.Instance.FileDoesNotExist(filePath);
                        return null;
                    }

                    else
                    {
                        return new FileInfo(filePath);
                    }
                });

            var startCommand = new Command("#!start-lesson")
            {
                fromFileOption,
                fromUrlOption
            };

            startCommand.Handler = CommandHandler.Create<Uri, FileInfo, KernelInvocationContext>(async (fromUrl, fromFile, context) =>
            {
                byte[] rawData = null;
                var name = "";
                if (fromFile is not null)
                {
                    rawData = await File.ReadAllBytesAsync(fromFile.FullName);
                    name = fromFile.Name;
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(fromUrl);
                    response.EnsureSuccessStatusCode();
                    rawData = await response.Content.ReadAsByteArrayAsync();
                    name = fromUrl.Segments.Last();
                }

                var document = NotebookFileFormatHandler.Parse(name, rawData, "csharp", new Dictionary<string, string>());
                NotebookLessonParser.Parse(document, out var lessonDefinition, out var challengeDefinitions);
                var challenges = challengeDefinitions.Select(b => b.ToChallenge()).ToList();
                challenges.SetDefaultProgressionHandlers();
                var lesson = lessonDefinition.ToLesson();
                lesson.SetChallengeLookup(name =>
                {
                    return challenges.FirstOrDefault(c => c.Name == name);
                });

                await lesson.StartChallengeAsync(challenges.First());

                await InitializeLesson(kernel, lesson);

                await kernel.Bootstrapping(lesson);

                await InitializeChallenge(kernel, lesson.CurrentChallenge);

                kernel.UseProgressiveLearningMiddleware(lesson);
            });

            kernel.AddDirective(startCommand);
            return kernel;
        }

        private static CompositeKernel UseProgressiveLearningMiddleware(this CompositeKernel kernel, Lesson lesson)
        {
            kernel.AddMiddleware(async (command, context, next) =>
            {
                switch (command)
                {
                    case SubmitCode submitCode:
                        var isSetupCommand = lesson.IsSetupCommand(submitCode);
                        var isModelAnswer = submitCode.Parent is not null
                            && submitCode.Parent is SubmitCode submitCodeParent
                            && submitCodeParent.Code.TrimStart().StartsWith(_modelAnswerCommandName);

                        if ((!lesson.IsTeacherMode && isSetupCommand)
                            || (lesson.IsTeacherMode && !isModelAnswer))
                        {
                            await next(command, context);
                            break;
                        }

                        var currentChallenge = lesson.CurrentChallenge;

                        var events = context.KernelEvents.ToSubscribedList();

                        await next(command, context);

                        await lesson.CurrentChallenge.Evaluate(submitCode.Code, events);
                        var view = currentChallenge.CurrentEvaluation.FormatAsHtml();
                        context.Display(view);

                        if (lesson.CurrentChallenge != currentChallenge)
                        {
                            if (lesson.IsTeacherMode)
                            {
                                await lesson.StartChallengeAsync(currentChallenge);
                            }
                            else
                            {
                                await InitializeChallenge(kernel, lesson.CurrentChallenge);
                            }
                        }

                        break;
                    default:
                        await next(command, context);
                        break;
                }
            });

            return kernel;
        }

        public static async Task InitializeChallenge(this Kernel kernel, Challenge challengeToInit)
        {
            if (challengeToInit is null)
            {
                return;
            }

            if (!challengeToInit.IsSetup)
            {
                foreach (var setup in challengeToInit.Setup)
                {
                    await kernel.SendAsync(setup);
                }
                challengeToInit.IsSetup = true;
            }

            foreach (var content in challengeToInit.Contents)
            {
                await kernel.SendAsync(content);
            }
            foreach (var setup in challengeToInit.EnvironmentSetup)
            {
                await kernel.SendAsync(setup);
            }
        }

        public static async Task Bootstrapping(this CompositeKernel kernel, Lesson lesson)
        {
            var k = kernel.RootKernel.FindKernel("csharp");

            await k.SubmitCodeAsync($"#r \"{typeof(Lesson).Assembly.Location}\"");

            k.DeferCommand(new SetVariableAsyncCommand("Lesson", lesson));

            if (k is DotNetKernel dotNetKernel)
            {
                await dotNetKernel.SetVariableAsync<Lesson>("Lesson", lesson);
            }
            else
            {
                throw new ArgumentException("no dotnet kernel");
            }

            if (lesson.IsTeacherMode)
            {
                lesson.ResetChallenge();
            }
        }

        private static async Task InitializeLesson(Kernel kernel, Lesson lesson)
        {
            lesson.ClearResetChallengeAction();
            lesson.IsTeacherMode = false;
            foreach (var setup in lesson.Setup)
            {
                await kernel.SendAsync(setup);
            }
        }
    }

    public class SetVariableAsyncCommand : KernelCommand
    {
        public SetVariableAsyncCommand(string name, object value, Type declaredType = null)
            : base(null, null)
        {
            Handler = async (command, context) =>
            {
                if (context.HandlingKernel is DotNetKernel k)
                {
                    await k.SetVariableAsync(name, value, declaredType);
                }
                else
                {
                    throw new Exception("Not dotnet kernel");
                }
            };
        }
    }
}
