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
        public Evaluation CurrentEvaluation => _submissionHistory.Peek().Evaluation;
        public ChallengeSubmission CurrentSubmission => _submissionHistory.Peek();
        public Stack<ChallengeSubmission> SubmissionHistory => _submissionHistory;

        private List<Rule> _rules = new();
        private Stack<ChallengeSubmission> _submissionHistory = new();
        private ChallengeContext _context;

        public Challenge(IReadOnlyList<EditableCode> content, Lesson lesson = null)
        {
            Contents = content;
            Lesson = lesson;
        }

        public async Task Evaluate(string submittedCode = null, IEnumerable<KernelEvent> events = null)
        {
            _submissionHistory.Push(new ChallengeSubmission(submittedCode, events));
            _context = new ChallengeContext(this);
            foreach (var (index, rule) in _rules.Select((r, i) => (i, r)))
            {
                var ruleContext = new RuleContext(this, _context, $"Rule {index + 1}");
                rule.Evaluate(ruleContext);
            }
            await InvokeOnCodeSubmittedHandler();
            if (!_context.IsChallengeOutcomeSet)
            {
                EvaluateChallengeEvaluationByDefault();
            }
        }

        // todo: rename
        public async Task InvokeOnCodeSubmittedHandler()
        {
            await OnCodeSubmittedHandler(_context);
        }

        private void EvaluateChallengeEvaluationByDefault()
        {
            // todo: discussion, is this right default behavior?

            var didAllRulesPass = CurrentEvaluation.RuleEvaluations
                .Select(e => e.Outcome).All(o => o == Outcome.Success);
            CurrentEvaluation.SetOutcome(didAllRulesPass ? Outcome.Success : Outcome.Failure);
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
