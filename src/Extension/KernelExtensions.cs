using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
using Microsoft.DotNet.Interactive.Parsing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Extension
{
    public static class KernelExtensions
    {
        private static string _modelAnswerCommandName = "#!model-answer";

        public static CompositeKernel UseModelAnswerValidation(this CompositeKernel kernel, Lesson lesson)
        {
            var modelAnswerCommand = new Command(_modelAnswerCommandName)
            {
                //Handler = CommandHandler.Create<KernelInvocationContext>(context =>
                //{
                //    var events = context.KernelEvents.ToSubscribedList();
                //    context.OnComplete(async invocationContext =>
                //    {
                //        if (context.Command is SubmitCode submitCode)
                //        {
                //            await lesson.CurrentChallenge.Evaluate(submitCode.Code, events);
                //            var view = lesson.CurrentChallenge.CurrentEvaluation.FormatAsHtml();
                //            context.Display(view);
                //        }
                //    });
                //})
            };
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
            var argument = new Argument<FileInfo>("file");

            var startCommand = new Command("#!start-lesson")
            {
                argument
            };

            startCommand.Handler = CommandHandler.Create<FileInfo, KernelInvocationContext>(async (file, context) =>
            {
                var rawData = await File.ReadAllBytesAsync(file.FullName);
                var document = kernel.ParseNotebook(file.Name, rawData);
                NotebookLessonParser.Parse(document, out var lessonDefinition, out var challengeDefinitions);

                var challenges = challengeDefinitions.Select(b => b.ToChallenge()).ToList();
                challenges.SetDefaultProgressionHandlers();
                var lesson = lessonDefinition.ToLesson();
                lesson.SetChallengeLookup(name =>
                {
                    return challenges.FirstOrDefault(c => c.Name == name);
                });

                await lesson.StartChallengeAsync(challenges.First());

                await InitializeLesson(lesson);

                await Bootstrapping(lesson);

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
            lesson.ClearResetChallengeAction();
            lesson.IsTeacherMode = false;
            foreach (var setup in lesson.Setup)
            {
                await Kernel.Root.SendAsync(setup);
            }
        }
    }
}
