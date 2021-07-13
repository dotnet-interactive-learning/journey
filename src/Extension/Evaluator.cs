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

        public Evaluation EvaluateResult(Banana result)
        {
            //var events = result.KernelEvents.ToEnumerable();
            //bool errorExists = events.Any(e => e is ErrorProduced || e is CommandFailed);
            //if (errorExists)
            //{
            //    return new Evaluation { Passed = false };

            //}

            var listOfRulePassOrFailOutcomes = new List<bool>();
            foreach (var rule in _rules){
                var banana = new Banana();
                rule.TestResult(banana);
                listOfRulePassOrFailOutcomes.Add(banana.Passed);
                
                   
            }
            if (listOfRulePassOrFailOutcomes.Contains(false))
            {
                return new Evaluation { Passed = false };

            }
            else { return new Evaluation { Passed = true }; }
            

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

            return new Evaluation { Passed = evaluationVerdict };
        }

        public void AddRule(Rule rule)
        {
            _rules.Add(rule);
          
        }

    }
}
