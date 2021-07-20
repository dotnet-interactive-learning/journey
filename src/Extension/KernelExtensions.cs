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
        public static T UseLessonEvaluateMiddleware<T>(this T kernel, Lesson lesson) where T : Kernel
        {
            kernel.AddMiddleware(async (command, context, next) =>
            {
                switch (command)
                {
                    case SubmitCode submitCode:
                        await next(command, context);
                        
                        var events = context.KernelEvents.ToSubscribedList();
                        await lesson.CurrentChallenge.Evaluate(submitCode.Code, events);
                        var view = lesson.CurrentChallenge.CurrentEvaluation.FormatAsHtml();
                        var formattedValues = FormattedValue.FromObject(view);
                        context.Publish(
                            new DisplayedValueProduced(
                                view,
                                command,
                                formattedValues));
                        break;
                    default:
                        await next(command, context);
                        break;
                }
            });
            return kernel;
        }
    }
}
