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
        public Lesson Lesson { get; internal set; }
        public IReadOnlyList<EditableCode> Contents { get; }
        public bool Revealed { get; set; } = false;
        public Func<ChallengeContext, Task> OnCodeSubmittedHandler { get; private set; }
        public ChallengeEvaluation CurrentEvaluation => CurrentSubmission?.Evaluation;
        public ChallengeSubmission CurrentSubmission => _submissionHistory.Count == 0 ? null : _submissionHistory.Peek();
        public IEnumerable<ChallengeSubmission> SubmissionHistory => _submissionHistory;

        private List<Rule> _rules = new();
        private Stack<ChallengeSubmission> _submissionHistory = new();
        private ChallengeContext _context;

        public Challenge(IReadOnlyList<EditableCode> content)
        {
            Contents = content;
        }

        public async Task Evaluate(string submittedCode = null, IEnumerable<KernelEvent> events = null)
        {
            _context = new ChallengeContext(this);

            foreach (var (rule, index) in _rules.Select((r, i) => (r, i)))
            {
                var ruleContext = new RuleContext(_context, submittedCode, events, $"Rule {index + 1}");
                try
                {
                    rule.Evaluate(ruleContext);
                }
                catch (Exception e)
                {

                    ruleContext.Fail(e.Message);
                }
            }

            await InvokeOnCodeSubmittedHandler();
            
            _submissionHistory.Push(new ChallengeSubmission(submittedCode, _context.Evaluation, events));
        }

        public async Task InvokeOnCodeSubmittedHandler()
        {
            if (OnCodeSubmittedHandler != null)
            {
                await OnCodeSubmittedHandler(_context);
            }
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
