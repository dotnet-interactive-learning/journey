namespace Extension
{
    public class RuleContext
    {
        public string Name { get; set; }
        public Challenge Challenge { get; private set; }
        public bool Passed { get; private set; } // do we need this?

        private ChallengeEvaluation _evaluation;

        public RuleContext()
        {

        }

        public RuleContext(Challenge challenge, ChallengeEvaluation evaluation, string defaultName = "")
        {
            Challenge = challenge;
            _evaluation = evaluation;
            Name = defaultName;
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
