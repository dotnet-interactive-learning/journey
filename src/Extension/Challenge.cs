using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
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
        public string Name { get; internal set; }
        public Lesson Lesson { get; internal set; }
        public IReadOnlyList<SendEditableCode> Contents { get; internal set; }
        public IReadOnlyList<SubmitCode> ChallengeSetup { get; internal set; }
        public bool Revealed { get; set; } = false;
        public Func<ChallengeContext, Task> OnCodeSubmittedHandler { get; private set; }
        public ChallengeEvaluation CurrentEvaluation => CurrentSubmission?.Evaluation;
        public ChallengeSubmission CurrentSubmission => _submissionHistory.Count == 0 ? null : _submissionHistory.Peek();
        public IEnumerable<ChallengeSubmission> SubmissionHistory => _submissionHistory;

        private List<Rule> _rules = new();
        private Stack<ChallengeSubmission> _submissionHistory = new();
        private ChallengeContext _context;

        public Challenge(IReadOnlyList<SendEditableCode> content = null, IReadOnlyList<SubmitCode> challengeSetup = null, string name = null)
        {
            Contents = content ?? new SendEditableCode[] { };
            ChallengeSetup = challengeSetup ?? new SubmitCode[] { };
            Name = name;
            OnCodeSubmittedHandler = (context) =>
            {
                KernelInvocationContext.Current?.Display(CurrentEvaluation);
                return Lesson.StartNextChallengeAsync();
            };
        }

        public async Task Evaluate(string submittedCode = null, IEnumerable<KernelEvent> events = null)
        {
            _context = new ChallengeContext(this);

            foreach (var rule in _rules)
            {
                var ruleContext = new RuleContext(_context, submittedCode, events, rule.Name);
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
            if (OnCodeSubmittedHandler is not null)
            {
                await OnCodeSubmittedHandler(_context);
            }
        }

        public void AddRuleAsync(string name, Func<RuleContext, Task> action)
        {
            name = string.IsNullOrWhiteSpace(name) ? $"Rule {_rules.Count + 1}" : name;
            AddRule(new Rule(action, name));
        }

        public void AddRuleAsync(Func<RuleContext, Task> action) => AddRuleAsync(null, action);

        public void AddRule(string name, Action<RuleContext> action)
        {
            AddRuleAsync(name, (context) =>
            {
                action(context);
                return Task.CompletedTask;
            });
        }

        public void AddRule(Action<RuleContext> action) => AddRule(null, action);

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
