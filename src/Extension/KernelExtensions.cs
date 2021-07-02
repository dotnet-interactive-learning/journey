using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Extension
{
    public static class KernelExtensions
    {
        public static T UseQuestionMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        {
            var cellIdArg = new Argument<int>("Question number");

            var questionCommand = new Command("#!question", "This cell will be evaluated")
            {
                cellIdArg
            };

            questionCommand.Handler = CommandHandler.Create<int, KernelInvocationContext>((id, context) =>
            {
                context.OnComplete(async (invocationContext) =>
                {
                    if (invocationContext.Command is SubmitCode submitCode)
                    {
                        var evaluationCriteria = evaluator.GetInputExecutionCriterion(id);
                        if (evaluationCriteria != "")
                        {
                            var result = await invocationContext.HandlingKernel.SubmitCodeAsync(evaluationCriteria);
                            var evaluation = evaluator.EvaluateInputExecution(result);
                            invocationContext.Publish(
                                new ExecutionEvaluationProduced(submitCode, evaluation));
                        }
                    }
                });
            });

            kernel.AddDirective(questionCommand);

            return kernel;
        }

        public static T UseAnswerMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        {
            var cellIdArg = new Argument<int>("Question number");
            var commandName = "#!answer";
            var answerCommand = new Command(commandName, "This cell contains code that evaluates a question")
            {
                cellIdArg
            };

            answerCommand.Handler = CommandHandler.Create<int, KernelInvocationContext>((id, context) =>
            {
                if (context.Command is SubmitCode submitCode)
                {
                    var newCode = "";
                    var lines = Regex.Split(submitCode.Code, "\r\n|\r|\n");
                    var filteredLines = lines.Where(line => !line.TrimStart().StartsWith(commandName));
                    newCode = string.Join(Environment.NewLine, filteredLines);

                    evaluator.AddInputExecutionCriterion(id, newCode);
                }
            });

            kernel.AddDirective(answerCommand);

            return kernel;
        }
    }
}
