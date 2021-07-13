using Microsoft.DotNet.Interactive.Formatting;
using System.Collections.Generic;

namespace Extension
{
    internal class EvaluationFormatterSource : ITypeFormatterSource
    {
        public IEnumerable<ITypeFormatter> CreateTypeFormatters()
        {
            return new ITypeFormatter[] {
                new HtmlFormatter<Evaluation>((evaluation, context) =>
                {
                    var view = evaluation.FormatAsHtml();
                    view.WriteTo(context);
                })
            };
        }
    }
}