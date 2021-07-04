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
        private SortedDictionary<string, List<CodeRunCriterion>> codeRunCriteria;
        private SortedDictionary<string, List<QuestionTextCriterion>> questionTextCriteria;

        public Evaluator()
        {
            codeRunCriteria = new SortedDictionary<string, List<CodeRunCriterion>>();
            questionTextCriteria = new SortedDictionary<string, List<QuestionTextCriterion>>();
        }

        public Evaluation EvaluateResult(KernelCommandResult result)
        {
            var events = result.KernelEvents.ToEnumerable();
            bool errorExists = events.Any(e => e is ErrorProduced || e is CommandFailed);
            if (errorExists)
            {
                return new Evaluation { Passed = false };

            }
            return new Evaluation { Passed = true };
        }

        public void AddCodeRunCriterion(string questionId, CodeRunCriterion criterion)
        {
            if (!codeRunCriteria.ContainsKey(questionId))
            {
                codeRunCriteria.Add(questionId, new List<CodeRunCriterion>());
            }
            codeRunCriteria[questionId].Add(criterion);
        }

        public IEnumerable<CodeRunCriterion> GetCodeRunCriteria(string questionId)
        {
            if (codeRunCriteria.ContainsKey(questionId))
            {
                return codeRunCriteria[questionId];
            }
            return Enumerable.Empty<CodeRunCriterion>();
        }

        public Evaluation EvaluateCodeRunResults(IEnumerable<KernelCommandResult> results)
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

        public void AddQuestionTextCriterion(string questionId, Predicate<string> criterion)
        {
            if (!questionTextCriteria.ContainsKey(questionId))
            {
                questionTextCriteria.Add(questionId, new List<QuestionTextCriterion>());
            }
            questionTextCriteria[questionId].Add(QuestionTextCriterion.FromPredicate(criterion));
        }

        public Evaluation EvaluateQuestionAsText(string questionId, string questionText)
        {
            if (!questionTextCriteria.ContainsKey(questionId))
            {
                return new Evaluation { Passed = true };
            }

            var predicateResults = questionTextCriteria[questionId]
                .Select(criterion => criterion.ToPredicate()(questionText));

            var evaluationVerdict = predicateResults
                .All(result => result);

            return new Evaluation { Passed = evaluationVerdict };
        }
    }
}
