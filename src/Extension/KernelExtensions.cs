using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
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
        //public static T UseQuestionMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        //{
        //    var questionCommand = new Command("#!question", "This question will be evaluated")
        //    {
        //        new Argument<string>("questionId", "Question number")
        //    };

        //    questionCommand.Handler = CommandHandler.Create<string, KernelInvocationContext>((questionId, context) =>
        //    {
        //        context.OnComplete(async (invocationContext) =>
        //        {
        //            if (invocationContext.Command is SubmitCode submitCode)
        //            {
        //                var evaluationCriteria = evaluator.GetCodeEvaluationCriteria(questionId);

        //                var results = await invocationContext.HandlingKernel.SubmitEvaluationCriteriaAsync(evaluationCriteria);

        //                var evaluation = evaluator.EvaluateCodeEvaluationResults(results);

        //                invocationContext.Publish(
        //                    new CodeEvaluationProduced(submitCode, evaluation));
        //            }
        //        });
        //    });

        //    kernel.AddDirective(questionCommand);

        //    return kernel;
        //}

        //public static T UseEvaluateMagicCommand<T>(this T kernel, Evaluator evaluator) where T : Kernel
        //{
        //    var commandName = "#!evaluate";
        //    var evaluateCommand = new Command(commandName, "The following code will evaluate a question")
        //    {
        //        new Argument<string>("questionid", "Question number")
        //    };

        //    evaluateCommand.Handler = CommandHandler.Create<string, KernelInvocationContext>((questionId, context) =>
        //    {
        //        if (context.Command is SubmitCode submitCode)
        //        {
        //            var newCode = "";
        //            var lines = Regex.Split(submitCode.Code, "\r\n|\r|\n");
        //            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith(commandName));
        //            newCode = string.Join(Environment.NewLine, filteredLines);

        //            evaluator.AddCodeEvaluationCriterion(questionId, CodeEvaluationCriterion.FromCodeString(newCode));
        //        }
        //    });

        //    kernel.AddDirective(evaluateCommand);

        //    return kernel;
        //}
    }
}
