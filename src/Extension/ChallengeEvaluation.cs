using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extension
{
    [TypeFormatterSource(typeof(ChallengeEvaluationFormatterSource))]
    public class ChallengeEvaluation
    {
        private readonly string label;

        private readonly Dictionary<string, RuleEvaluation> ruleEvaluations = new();

        public IEnumerable<RuleEvaluation> RuleEvaluations => ruleEvaluations.Values;

        public string Message { get; private set; }

        public object Hint { get; private set; }

        public ChallengeEvaluation(string label = null)
        {
            this.label = label;
        }

        public void SetMessage(string message, object hint = null)
        {
            Hint = hint;
            Message = message;
        }

        public PocketView FormatAsHtml()
        {
            var elements = new List<PocketView>();
            var succeededRules = ruleEvaluations.Values.Count(r => r.Outcome == Outcome.Success);
            var totalRules = ruleEvaluations.Count;
            var countReport = totalRules > 0 ? $"({succeededRules}/{totalRules})" : string.Empty;
            var message = string.IsNullOrWhiteSpace(Message) ? $"{countReport} passed" : Message;

            if (string.IsNullOrWhiteSpace(label))
            {
                PocketView summary = div[@class: "summary"](b(message));

                elements.Add(summary);
            }
            else
            {
                PocketView summary = div[@class: "summary"](b($"[ {this.label} ] "), b(message));

                elements.Add(summary);
            }

            if (Hint is not null)
            {
                var hintElement = div[@class: "hint"](Hint.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent());
                elements.Add(hintElement);
            }
            foreach (var rule in ruleEvaluations.Values.OrderBy(r => r.Outcome).ThenBy(r => r.Label))
            {
                elements.Add(div[@class: "rule"](rule.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent()));
            }

            PocketView report = div(elements);

            return report;
        }

        public void SetRuleOutcome(string name, Outcome outcome, string reason = null, object hint = null)
        {
            var ruleEvaluation = new RuleEvaluation(name);
            ruleEvaluation.SetOutcome(outcome, reason, hint);
            ruleEvaluations[name] = ruleEvaluation;
        }
    }
}
