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
using Microsoft.DotNet.Interactive.CSharp;

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

        public static CompositeKernel UseProgressiveLearning(this CompositeKernel kernel)
        {
            var fromUrlOption = new Option<Uri>(
                "--from-url",
                "Specify lesson source URL");

            var fromFileOption = new Option<FileInfo>(
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

            startCommand.Handler = CommandHandler.Create<Uri, FileInfo, KernelInvocationContext>(StartCommandHandler);

            kernel.AddDirective(startCommand);
            return kernel;

            async Task StartCommandHandler(Uri fromUrl, FileInfo fromFile, KernelInvocationContext context)
            {
                byte[] rawData;
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

                var document = kernel.ParseNotebook(name, rawData);
                NotebookLessonParser.Parse(document, out var lessonDefinition, out var challengeDefinitions);
                var challenges = challengeDefinitions.Select(b => b.ToChallenge()).ToList();
                challenges.SetDefaultProgressionHandlers();
                Lesson.From(lessonDefinition);
                Lesson.SetChallengeLookup(queryName =>
                {
                    return challenges.FirstOrDefault(c => c.Name == queryName);
                });

                await kernel.StartLesson();

                await Lesson.StartChallengeAsync(challenges.First());

                await kernel.Bootstrapping();

                await kernel.InitializeChallenge(Lesson.CurrentChallenge);
            }
        }

        public static CompositeKernel UseProgressiveLearningMiddleware(this CompositeKernel kernel)
        {
            kernel.AddMiddleware(async (command, context, next) =>
            {
                switch (command)
                {
                    case SubmitCode submitCode:
                        var isSetupCommand = Lesson.IsSetupCommand(submitCode);
                        var isModelAnswer = submitCode.Parent is SubmitCode submitCodeParent
                            && submitCodeParent.Code.TrimStart().StartsWith(_modelAnswerCommandName);

                        if (Lesson.Mode == LessonMode.StudentMode && isSetupCommand
                            || Lesson.Mode == LessonMode.TeacherMode && !isModelAnswer)
                        {
                            await next(command, context);
                            break;
                        }

                        var currentChallenge = Lesson.CurrentChallenge;

                        var events = context.KernelEvents.ToSubscribedList();

                        await next(command, context);

                        await Lesson.CurrentChallenge.Evaluate(submitCode.Code, events);
                        var view = currentChallenge.CurrentEvaluation.FormatAsHtml();
                        context.Display(view);

                        if (Lesson.CurrentChallenge != currentChallenge)
                        {
                            switch (Lesson.Mode)
                            {
                                case LessonMode.StudentMode:
                                    await InitializeChallenge(kernel, Lesson.CurrentChallenge);
                                    break;
                                case LessonMode.TeacherMode:
                                    await Lesson.StartChallengeAsync(currentChallenge);
                                    break;
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

        public static async Task Bootstrapping(this CompositeKernel kernel)
        {
            var csharpKernel = kernel.RootKernel.FindKernel("csharp") as CSharpKernel;
            await csharpKernel.SubmitCodeAsync($"#r \"{typeof(Lesson).Assembly.Location}\"");
            await csharpKernel.SubmitCodeAsync($"using {typeof(Lesson).Namespace};");
        }

        private static async Task StartLesson(this Kernel kernel)
        {
            Lesson.Mode = LessonMode.StudentMode;
            foreach (var setup in Lesson.Setup)
            {
                await kernel.SendAsync(setup);
            }
        }
    }
}
