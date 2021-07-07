using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;

namespace Extension
{
    [TypeFormatterSource(typeof(EvaluationFormatterSource))]
    public class Evaluation
    {
        public Evaluation(bool passed, string headline = null)
        {
            Passed = passed;
            Headline = headline;
        }

        public bool Passed { get; }
        public string Headline { get; }

        public string FormatAsHtml()
        {
            var headlineMessage = Headline ?? (Passed ? "Success" : "Fail");
            return headlineMessage;
        }
    }

    internal class EvaluationFormatterSource : ITypeFormatterSource
    {
        public IEnumerable<ITypeFormatter> CreateTypeFormatters()
        {
            return new ITypeFormatter[] {
                new HtmlFormatter<Evaluation>((evaluation, context) =>
                {
                    context.Writer.Write(evaluation.FormatAsHtml());
                })
            };
        }
    }
}