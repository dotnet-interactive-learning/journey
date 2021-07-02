using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;

namespace Extension.Tests
{
    [TypeFormatterSource(typeof(EvaluationFormatterSource))]
    public class Evaluation
    {
        public Evaluation()
        {
        }

        public bool Passed { get; set; }

        public string FormatAsHtml()
        {
            if (Passed)
                return "Success";

            return "Fail";
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