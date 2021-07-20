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
        public string Name { get; }
        public Lesson Lesson { get; internal set; }
        public IReadOnlyList<EditableCode> Contents { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }
        public bool Revealed { get; set; } = false;
        public Func<ChallengeContext, Task> OnCodeSubmittedHandler { get; private set; }
        public ChallengeEvaluation CurrentEvaluation => CurrentSubmission?.Evaluation;
        public ChallengeSubmission CurrentSubmission => _submissionHistory.Count == 0 ? null : _submissionHistory.Peek();
        public IEnumerable<ChallengeSubmission> SubmissionHistory => _submissionHistory;

        private List<Rule> _rules = new();
        private Stack<ChallengeSubmission> _submissionHistory = new();
        private ChallengeContext _context;

        public Challenge(IReadOnlyList<EditableCode> content, IReadOnlyList<SubmitCode> setup = null, string name = null)
        {
            Contents = content;
            Setup = setup ?? new SubmitCode[] { };
            Name = name;
        }

        public async Task Evaluate(string submittedCode = null, IEnumerable<KernelEvent> events = null)
        {
            _context = new ChallengeContext(this);

            foreach (var (rule, index) in _rules.Select((r, i) => (r, i)))
            {
                var ruleLabel = string.IsNullOrWhiteSpace(rule.Name) ? $"Rule {index + 1}" : rule.Name;
                var ruleContext = new RuleContext(_context, submittedCode, events, ruleLabel);
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
            if (Lesson.CurrentChallenge == this)
            {
                if (_context.RuleEvaluations.All(e => e.Passed))
                {
                    await Lesson.StartNextChallengeAsync();
                }
            }

            _submissionHistory.Push(new ChallengeSubmission(submittedCode, _context.Evaluation, events));
        }

        public async Task InvokeOnCodeSubmittedHandler()
        {
            if (OnCodeSubmittedHandler is not null)
            {
                await OnCodeSubmittedHandler(_context); 
            }
        }

        public void AddRuleAsync(string name, Func<RuleContext, Task> action) => AddRule(new Rule(action, name));

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
