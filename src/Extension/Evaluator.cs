using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Extension.Criterion;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Events;

namespace Extension
{
    public class Evaluator
    {
        private List<Rule> _rules = new();
        private SortedDictionary<string, List<CodeEvaluationCriterion>> codeEvaluationCriteria;

        public Evaluator()
        {
            codeEvaluationCriteria = new SortedDictionary<string, List<CodeEvaluationCriterion>>();
        }

        public Evaluation EvaluateResult(RuleContext result)
        {
            var evaluation = new Evaluation();

            var listOfRulePassOrFailOutcomes = new List<bool>();
            foreach (var rule in _rules){
                var ruleContext = new RuleContext();
                rule.TestResult(ruleContext);
                listOfRulePassOrFailOutcomes.Add(ruleContext.Passed); 
            }

            if (listOfRulePassOrFailOutcomes.Contains(false))
            {
                evaluation.SetOutcome(Outcome.Failure);
            }
            else
            {
                evaluation.SetOutcome(Outcome.Success);
            }
            return evaluation;
        }

        public void AddCodeEvaluationCriterion(string questionId, CodeEvaluationCriterion criterion)
        {
            if (!codeEvaluationCriteria.ContainsKey(questionId))
            {
                codeEvaluationCriteria.Add(questionId, new List<CodeEvaluationCriterion>());
            }
            codeEvaluationCriteria[questionId].Add(criterion);
        }

        public IEnumerable<CodeEvaluationCriterion> GetCodeEvaluationCriteria(string questionId)
        {
            if (codeEvaluationCriteria.ContainsKey(questionId))
            {
                return codeEvaluationCriteria[questionId];
            }
            return Enumerable.Empty<CodeEvaluationCriterion>();
        }

        public Evaluation EvaluateCodeEvaluationResults(IEnumerable<KernelCommandResult> results)
        {
            var events = results.Select(result => result.KernelEvents.ToEnumerable());

            var foundReturnValueProducedEvents = events
                .Select(evts => evts.FirstOrDefault(e => e is ReturnValueProduced))
                .Where(e => e is { })
                .Select(e => (ReturnValueProduced)e);
                
            var evaluationVerdict = foundReturnValueProducedEvents.All(
                (returnValueProduced) =>
                {
                    var value = returnValueProduced.Value;
                    return !(value is bool) || (bool)value;
                }
            );

            var evaluation = new Evaluation();
            if (evaluationVerdict)
            {
                evaluation.SetOutcome(Outcome.Success);
            }
            else
            {
                evaluation.SetOutcome(Outcome.Failure);
            }
            return evaluation;
        }

        public void AddRule(Rule rule)
        {
            _rules.Add(rule);
          
        }

    }
}
