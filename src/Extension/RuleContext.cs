using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;

namespace Extension
{
    public class RuleContext
    {
        public string Name { get; set; }
        public Challenge Challenge => _challenge;
        public Outcome? RuleOutcome => _challengeEvaluation.GetRuleEvaluation(Name)?.Outcome;
        public bool Passed => RuleOutcome == Outcome.Success;
        public string SubmittedCode => _submission.SubmittedCode;
        public IEnumerable<KernelEvent> EventsProduced => _submission.EventsProduced;

        private Evaluation _challengeEvaluation => _challengeContext.Evaluation;
        private readonly Challenge _challenge;
        private readonly ChallengeContext _challengeContext;
        private readonly ChallengeSubmission _submission;

        internal RuleContext(Challenge challenge, ChallengeContext challengeContext, string defaultName = "")
        {
            _challenge = challenge;
            _challengeContext = challengeContext;
            Name = defaultName;
            _submission = _challenge.CurrentSubmission;
        }

        public void Fail(string reason = null, object hint = null)
        {
            _challengeEvaluation?.SetRuleOutcome(Name, Outcome.Failure, reason, hint);
        }

        public void Pass(string reason = null, object hint = null)
        {
            _challengeEvaluation?.SetRuleOutcome(Name, Outcome.Success, reason, hint);
        }

        public void PartialPass(string reason = null, object hint = null)
        {
            _challengeEvaluation?.SetRuleOutcome(Name, Outcome.PartialSuccess, reason, hint);
        }
    }
}
