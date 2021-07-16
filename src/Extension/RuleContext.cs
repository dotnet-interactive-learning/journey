using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;

namespace Extension
{
    public class RuleContext
    {
        public string Name { get; set; }
        public Challenge Challenge => _challenge;
        public bool Passed { get; private set; } // do we need this?
        public string SubmittedCode => _submission.SubmittedCode;
        public IEnumerable<KernelEvent> EventsProduced => _submission.EventsProduced;

        private Evaluation _evaluation;
        private readonly Challenge _challenge;
        private readonly ChallengeSubmission _submission;

        public RuleContext()
        {

        }

        internal RuleContext(Challenge challenge, Evaluation evaluation, string defaultName = "")
        {
            _challenge = challenge;
            _evaluation = evaluation;
            Name = defaultName;
            _submission = _challenge.CurrentSubmission;
        }

        public void Fail(string reason = null, object hint = null)
        {
            Passed = false;
            _evaluation?.SetRuleOutcome(Name, Outcome.Failure, reason, hint);
        }

        public void Pass(string reason = null, object hint = null)
        {
            Passed = true;
            _evaluation?.SetRuleOutcome(Name, Outcome.Success, reason, hint);
        }

        public void PartialPass(string reason = null, object hint = null)
        {
            _evaluation?.SetRuleOutcome(Name, Outcome.PartialSuccess, reason, hint);
        }
    }
}
