using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeContext
    {
        public Lesson Lesson => _challenge.Lesson;
        public Evaluation Evaluation => _challenge.CurrentEvaluation;
        public Stack<ChallengeSubmission> SubmissionHistory => _challenge.SubmissionHistory;
        public IEnumerable<Evaluation> RuleEvaluations => Evaluation.RuleEvaluations;

        internal bool IsChallengeOutcomeSet => Evaluation.Outcome != Outcome.Unset;

        private readonly Challenge _challenge;

        internal ChallengeContext(Challenge challenge)
        {
            if (challenge is null)
            {
                throw new ArgumentNullException(nameof(challenge));
            }
            _challenge = challenge;
        }

        public void SetOutcome(Outcome outcome, string reason = null, object hint = null)
        {
            Evaluation.SetOutcome(outcome, reason, hint);
        }

        // todo: get rid of these 
        public void Pass(string reason = null, object hint = null)
        {
            Evaluation.SetOutcome(Outcome.Success, reason, hint);
        }

        public void Fail(string reason = null, object hint = null)
        {
            Evaluation.SetOutcome(Outcome.Failure, reason, hint);
        }

        public void PartialPass(string reason = null, object hint = null)
        {
            Evaluation.SetOutcome(Outcome.PartialSuccess, reason, hint);
        }

        public async Task StartChallengeAsync(Challenge challenge)
        {
            await Lesson.StartChallengeAsync(challenge);
        }
    }
}
