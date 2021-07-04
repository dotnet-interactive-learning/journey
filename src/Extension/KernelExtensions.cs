using Extension.Criterion;
using Extension.Events;
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
        public static Task<KernelCommandResult[]> SubmitEvaluationCriteriaAsync<T>(
            this T kernel, 
            IEnumerable<CodeRunCriterion> evaluationCriteria)
            where T : Kernel
        {
            return Task.WhenAll(
                evaluationCriteria
                    .Select(criterion => kernel.SubmitCodeAsync(criterion.ToCodeString()))
            );
        }

        public static T UseQuestionMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        {
            var questionCommand = new Command("#!question", "This cell will be evaluated")
            {
                new Argument<string>("questionId", "Question number")
            };

            questionCommand.Handler = CommandHandler.Create<string, KernelInvocationContext>((questionId, context) =>
            {
                if (context.Command is SubmitCode submitCode)
                {
                    var evaluation = evaluator.EvaluateQuestionAsText(questionId, submitCode.Code);
                    context.Publish(
                        new QuestionTextEvaluationProduced(submitCode, evaluation));
                }

                context.OnComplete(async (invocationContext) =>
                {
                    if (invocationContext.Command is SubmitCode submitCode)
                    {
                        var evaluationCriteria = evaluator.GetCodeRunCriteria(questionId);

                        var results = await invocationContext.HandlingKernel.SubmitEvaluationCriteriaAsync(evaluationCriteria);

                        var evaluation = evaluator.EvaluateCodeRunResults(results);

                        invocationContext.Publish(
                            new CodeRunEvaluationProduced(submitCode, evaluation));
                    }
                });
            });

            kernel.AddDirective(questionCommand);

            return kernel;
        }

        public static T UseAnswerMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        {
            var commandName = "#!answer";
            var answerCommand = new Command(commandName, "This cell contains code that evaluates a question")
            {
                new Argument<string>("questionid", "Question number")
            };

            answerCommand.Handler = CommandHandler.Create<string, KernelInvocationContext>((questionId, context) =>
            {
                if (context.Command is SubmitCode submitCode)
                {
                    var newCode = "";
                    var lines = Regex.Split(submitCode.Code, "\r\n|\r|\n");
                    var filteredLines = lines.Where(line => !line.TrimStart().StartsWith(commandName));
                    newCode = string.Join(Environment.NewLine, filteredLines);

                    evaluator.AddCodeRunCriterion(questionId, CodeRunCriterion.FromCodeString(newCode));
                }
            });

            kernel.AddDirective(answerCommand);

            return kernel;
        }
    }
}
