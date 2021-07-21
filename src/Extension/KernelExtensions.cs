using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
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

            async Task InitializeChallenge(Challenge challengeToInit)
            {
                foreach (var content in challengeToInit.Contents)
                {
                    await Kernel.Root.SendAsync(content);
                }
                foreach (var setup in challengeToInit.ChallengeSetup)
                {
                    await Kernel.Root.SendAsync(setup);
                }
            }
        }
    }
}
