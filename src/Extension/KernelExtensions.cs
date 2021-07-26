using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
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
        public static T UseProgressiveLearning<T>(this T kernel, Lesson lesson) where T : Kernel
        {
            var startCommand = new Command("#!start-lesson");
            startCommand.Handler = CommandHandler.Create<KernelInvocationContext>(async context =>
            {
                await lesson.StartLessonAsync();
                await InitializeChallenge(lesson.CurrentChallenge);
            });
            kernel.AddDirective(startCommand);
            kernel.UseProgressiveLearningMiddleware<T>(lesson);
            return kernel;
        }

        public static T UseProgressiveLearning<T>(this T kernel) where T : Kernel, IKernelCommandHandler<ParseNotebook>
        {
            Option<Uri> fromUrlOption = new Option<Uri>(
                "--from-url",
                "Specify lesson source URL" );

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

                // todo: NotebookFileFormatHandler.Parse what are its last two arguments
                var document = NotebookFileFormatHandler.Parse(name, rawData, "csharp", new Dictionary<string, string>());
                var lessonBlueprint = NotebookLessonParser.Parse(document);
                var lesson = lessonBlueprint.ToLesson();

                await InitializeLesson(lesson);

                await lesson.StartLessonAsync();

                await Bootstrapping(lesson);

                await InitializeChallenge(lesson.CurrentChallenge);

                kernel.UseProgressiveLearningMiddleware<T>(lesson);
            });

            kernel.AddDirective(startCommand);
            return kernel;
        }

        private static T UseProgressiveLearningMiddleware<T>(this T kernel, Lesson lesson) where T : Kernel
        {
            kernel.AddMiddleware(async (command, context, next) =>
            {
                switch (command)
                {
                    case SubmitCode submitCode:
                        if (lesson.IsSetupCommand(submitCode))
                        {
                            await next(command, context);
                            break;
                        }
                        var currentChallenge = lesson.CurrentChallenge;

                        await next(command, context);

                        var events = context.KernelEvents.ToSubscribedList();
                        await lesson.CurrentChallenge.Evaluate(submitCode.Code, events);
                        var view = currentChallenge.CurrentEvaluation.FormatAsHtml();
                        var formattedValues = FormattedValue.FromObject(view);
                        context.Publish(
                            new DisplayedValueProduced(
                                view,
                                command,
                                formattedValues));

                        if (lesson.CurrentChallenge != currentChallenge)
                        {
                            await InitializeChallenge(lesson.CurrentChallenge);
                        }

                        break;
                    default:
                        await next(command, context);
                        break;
                }
            });

            return kernel;
        }

        private static async Task InitializeChallenge(Challenge challengeToInit)
        {
            if (challengeToInit is null)
            {
                return;
            }

            if (!challengeToInit.IsSetup)
            {
                foreach (var setup in challengeToInit.Setup)
                {
                    await Kernel.Root.SendAsync(setup);
                }
                challengeToInit.IsSetup = true;
            }

            foreach (var content in challengeToInit.Contents)
            {
                await Kernel.Root.SendAsync(content);
            }
            foreach (var setup in challengeToInit.EnvironmentSetup)
            {
                await Kernel.Root.SendAsync(setup);
            }
        }

        private static async Task Bootstrapping(Lesson lesson)
        {
            var kernel = Kernel.Root.FindKernel("csharp");
            await kernel.SubmitCodeAsync($"#r \"{typeof(Lesson).Assembly.Location}\"");
            await kernel.SubmitCodeAsync($"#r \"{typeof(Lesson).Namespace}\"");
            if (kernel is DotNetKernel dotNetKernel)
            {
                await dotNetKernel.SetVariableAsync<Lesson>("Lesson", lesson);
            }
        }

        private static async Task InitializeLesson(Lesson lesson)
        {
            foreach (var setup in lesson.Setup)
            {
                await Kernel.Root.SendAsync(setup);
            }
        }
    }
}
