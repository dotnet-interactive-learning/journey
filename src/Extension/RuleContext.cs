namespace Extension
{
    public class RuleContext
    {
        private readonly ChallengeContext _challengeContext;
        public string Name { get; set; }
        public Challenge Challenge => _challengeContext.Challenge;
        public bool Passed { get; private set; } // do we need this?

        public RuleContext(ChallengeContext challengeContext, string defaultName = "")
        {
            _challengeContext = challengeContext;
            Name = defaultName;
        }

        public void Fail(string reason = null, object hint = null)
        {
            Passed = false;
            _challengeContext.Evaluation?.SetRuleOutcome(Name, Outcome.Failure, reason, hint);
        }

        public void Pass(string reason = null, object hint = null)
        {
            Passed = true;
            _challengeContext.Evaluation?.SetRuleOutcome(Name, Outcome.Success, reason, hint);
        }

        public void PartialPass(string reason = null, object hint = null)
        {
            _challengeContext.Evaluation?.SetRuleOutcome(Name, Outcome.PartialSuccess, reason, hint);
        }
    }
}
