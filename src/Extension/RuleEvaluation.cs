using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interactive.Journey
{
    public enum Outcome
    {
        Failure,
        PartialSuccess,
        Success
    };
    [TypeFormatterSource(typeof(RuleEvaluationFormatterSource))]
    public class RuleEvaluation
    {
        public string Name { get; }

        public Outcome Outcome { get; }

        public string Reason { get; }

        public object Hint { get; }

        public bool Passed { get { return Outcome == Outcome.Success; } }

        public RuleEvaluation(Outcome outcome, string name = null, string reason = null, object hint = null)
        {
            Name = name;
            Hint = hint;
            Outcome = outcome;
            if (string.IsNullOrWhiteSpace(reason))
            {
                Reason = outcome switch
                {
                    Outcome.Success => "All tests passed.",
                    Outcome.PartialSuccess => "Some tests passed.",
                    Outcome.Failure => "Incorrect solution.",
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                Reason = reason;
            }
        }

        public PocketView FormatAsHtml()
        {
            var outcomeDivStyle = Outcome switch
            {
                Outcome.Success => "background:#49B83461; border-width:thin; border-color:#49B83461",
                Outcome.PartialSuccess => "background:#FF00008A; border-width:thin; border-color:#FF00008A",
                Outcome.Failure => "background:#FF00008A; border-width:thin; border-color:#FF00008A",
                _ => throw new NotImplementedException()
            };

            var outcomeMessage = Outcome switch
            {
                Outcome.Success => "Success",
                Outcome.PartialSuccess => "Partial Success",
                Outcome.Failure => "Failure",
                _ => throw new NotImplementedException()
            };

            var elements = new List<PocketView>();

            if (string.IsNullOrWhiteSpace(Name))
            {
                PocketView header = summary[style: outcomeDivStyle](b(outcomeMessage));

                elements.Add(header);

            }
            else
            {
                PocketView header = summary[style: outcomeDivStyle](($"[ {Name} ]: "), b(outcomeMessage));

                elements.Add(header);
            }

            if (!string.IsNullOrWhiteSpace(Reason))
            {
                elements.Add(div(Reason)); 
            }

            if (Hint is not null)
            {
                var hintElement = div[@class: "hint"](b("Hint: "), Hint.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent());
                elements.Add(hintElement);
            }

            PocketView report = details[@class: "ruleEvaluation"](elements);

            return report;
        }
    }
}