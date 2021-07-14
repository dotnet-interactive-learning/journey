using Extension.Criterion;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class Challenge
    {
        public Lesson Lesson { get; set; }
        public IReadOnlyList<EditableCode> Contents { get; private set; }
        public bool Passed { get; private set; } = false;
        public bool Revealed { get; private set; } = false;
        public List<Challenge> Dependencies { get; private set; } = new List<Challenge>();
        public List<Challenge> Dependents { get; private set; } = new List<Challenge>();
        public Func<ChallengeContext, Task> OnCodeSubmittedHandler { get; private set; }

        private List<Rule> _rules = new();
        private SortedDictionary<string, List<CodeEvaluationCriterion>> codeEvaluationCriteria;

        private List<Action<Challenge>> _onRevealListeners = new List<Action<Challenge>>();
        private List<Action<Challenge>> _onFocusListeners = new List<Action<Challenge>>();

        public Challenge(IReadOnlyList<EditableCode> content, Lesson lesson = null)
        {
            Contents = content;
            Lesson = lesson;
        }

        public void AddDependency(Challenge challenge)
        {
            Dependencies.Add(challenge);
        }

        public void AddDependent(Challenge challenge)
        {
            Dependents.Add(challenge);
        }

        public void AddOnRevealListener(Action<Challenge> listener)
        {
            _onRevealListeners.Add(listener);
        }

        public void AddOnFocusListener(Action<Challenge> listener)
        {
            _onFocusListeners.Add(listener);
        }

        public void Pass()
        {
            Passed = true;
            foreach (var dependent in Dependents)
            {
                if (dependent.Revealed || dependent.CanReveal())
                {
                    dependent.Focus();
                }
            }
        }

        public void Focus()
        {
            foreach (var listener in _onFocusListeners)
            {
                listener(this);
            }
            Reveal();
        }
        public bool CanReveal()
        {
            return Dependencies
                .Select(dependency => dependency.Passed)
                .All(passed => passed);
        }

        public void Reveal()
        {
            if (!Revealed)
            {
                foreach (var listener in _onRevealListeners)
                {
                    listener(this);
                }
                Revealed = true; 
            }
        }

        public void ClearDependencyRelationships()
        {
            Dependencies.Clear();
            Dependents.Clear();
        }

        public void OnCodeSubmitted(Func<ChallengeContext, Task> handler)
        {
            OnCodeSubmittedHandler = handler;
        }

        public async Task InvokeOnEvaluationComplete()
        {
            await OnCodeSubmittedHandler();
        }

        public Evaluation EvaluateResult(RuleContext result)
        {
            var evaluation = new Evaluation();

            var listOfRulePassOrFailOutcomes = new List<bool>();
            foreach (var rule in _rules)
            {
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

        public void AddRule(Func<RuleContext, Task> action)
        {
            AddRule(new Rule(action));
        }

    }
}
