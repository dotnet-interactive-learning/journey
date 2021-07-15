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
        public bool Revealed { get; set; } = false;
        public Func<ChallengeContext, Task> OnCodeSubmittedHandler { get; private set; }

        private List<Rule> _rules = new();

        private ChallengeContext context;

        public Challenge(IReadOnlyList<EditableCode> content, Lesson lesson = null)
        {
            Contents = content;
            Lesson = lesson;
            context = new ChallengeContext(lesson);
        }

        public async Task InvokeOnEvaluationComplete()
        {
            await OnCodeSubmittedHandler(context);
        }

        public Evaluation EvaluateByDefault(RuleContext result)
        {
            // todo: result unused
            // prob remove this arg because 
            // we'll use challenge info in this object to construct rulecontext
            // to pass them into rule.Evaluate()
            var evaluation = new Evaluation();

            var listOfRulePassOrFailOutcomes = new List<bool>();
            foreach (var rule in _rules)
            {
                var ruleContext = new RuleContext();
                rule.Evaluate(ruleContext);
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

        public void AddRuleAsync(Func<RuleContext, Task> action)
        {
            AddRule(new Rule(action));
        }

        public void AddRule(Action<RuleContext> action)
        {
            AddRuleAsync((context) =>
            {
                action(context);
                return Task.CompletedTask;
            });
        }

        public void OnCodeSubmittedAsync(Func<ChallengeContext, Task> action)
        {
            OnCodeSubmittedHandler = action;
        }

        public void OnCodeSubmitted(Action<ChallengeContext> action)
        {
            OnCodeSubmittedAsync((context) =>
            {
                action(context);
                return Task.CompletedTask;
            });
        }

        private void AddRule(Rule rule)
        {
            _rules.Add(rule);
        }
    }
}
