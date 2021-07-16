using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeContext
    {
        public Lesson Lesson => _challenge.Lesson;
        public ChallengeEvaluation Evaluation => _challenge.CurrentEvaluation;
        public IEnumerable<ChallengeSubmission> SubmissionHistory => _challenge.SubmissionHistory;

        private readonly Challenge _challenge;

        public IEnumerable<RuleEvaluation> RuleEvaluations
        {
            get => Evaluation.RuleEvaluations;
        }

        internal ChallengeContext(Challenge challenge)
        {
            if (challenge is null)
            {
                throw new ArgumentNullException(nameof(challenge));
            }
            _challenge = challenge;
        }

        public void SetMessage(string message, object hint = null)
        {
            Evaluation.SetMessage(message, hint);
        }

        public async Task StartChallengeAsync(Challenge challenge)
        {
            await Lesson.StartChallengeAsync(challenge);
        }
    }
}
