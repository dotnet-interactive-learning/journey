using Microsoft.DotNet.Interactive.Formatting;
using System.Collections.Generic;

namespace Extension
{
    internal class RuleEvaluationFormatterSource : ITypeFormatterSource
    {
        public IEnumerable<ITypeFormatter> CreateTypeFormatters()
        {
            return new ITypeFormatter[] {
                new HtmlFormatter<RuleEvaluation>((evaluation, context) =>
                {
                    var view = evaluation.FormatAsHtml();
                    view.WriteTo(context);
                })
            };
        }
    }

    internal class ChallengeEvaluationFormatterSource : ITypeFormatterSource
    {
        public IEnumerable<ITypeFormatter> CreateTypeFormatters()
        {
            return new ITypeFormatter[] {
                new HtmlFormatter<ChallengeEvaluation>((evaluation, context) =>
                {
                    var view = evaluation.FormatAsHtml();
                    view.WriteTo(context);
                })
            };
        }
    }
}