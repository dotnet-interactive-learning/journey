using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extension
{
    public enum Outcome
    {
        Unset,
        Failure,
        PartialSuccess,
        Success
    };
    [TypeFormatterSource(typeof(EvaluationFormatterSource))]
    // make immutable
    // recursively defined?
    public class Evaluation
    {

        public IEnumerable<Evaluation> RuleEvaluations => _ruleEvaluations.Values;

        public Outcome Outcome { get; private set; }

        public string Reason { get; private set; }

        public object Hint { get; private set; }

        public bool Passed { get { return Outcome == Outcome.Success; } }

        private readonly string _label;

        private readonly Dictionary<string, Evaluation> _ruleEvaluations = new();


        public Evaluation(string label = null)
        {
            this._label = label;
        }

        public void SetOutcome(Outcome outcome, string reason = null, object hint = null)
        {
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
                Outcome.Success => "background:green",
                Outcome.PartialSuccess => "background:#eb6f00",
                Outcome.Failure => "background:red",
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
            var succeededRules = _ruleEvaluations.Values.Count(r => r.Outcome == Outcome.Success);
            var totalRules = _ruleEvaluations.Count;
            var countReport = totalRules > 0 ? $"({succeededRules}/{totalRules})" : string.Empty;
            outcomeMessage = $"{outcomeMessage}{countReport}: ";

            if (string.IsNullOrWhiteSpace(_label))
            {
                PocketView summary = div[@class: "summary", style: outcomeDivStyle](b(outcomeMessage), (Reason));

                elements.Add(summary);

            }
            else
            {
                PocketView summary = div[@class: "summary", style: outcomeDivStyle](b($"[ {this._label} ] "), b(outcomeMessage), (Reason));

                elements.Add(summary);
            }
            
            if (Hint is not null)
            {
                var hintElement = div[@class: "hint"](Hint.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent());
                elements.Add(hintElement);
            }
            foreach (var rule in _ruleEvaluations.Values.OrderBy(r=>r.Outcome).ThenBy(r=>r._label))
            {
                elements.Add(div[@class: "rule"](rule.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent()));
            }

            PocketView report = div(elements);

            return report;
        }

        public void SetRuleOutcome(string name, Outcome outcome, string reason = null, object hint = null)
        {
            var ruleEvaluation = new Evaluation(name);
            ruleEvaluation.SetOutcome(outcome, reason, hint);
            _ruleEvaluations[name] = ruleEvaluation;
        }

        public Evaluation GetRuleEvaluation(string name)
        {
            if (!_ruleEvaluations.ContainsKey(name))
            {
                return null;
            }
            return _ruleEvaluations[name];
        }
    }
}