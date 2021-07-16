using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeContext
    {
        public Lesson Lesson => Challenge.Lesson;
        public ChallengeEvaluation Evaluation { get; }
        public IEnumerable<ChallengeSubmission> SubmissionHistory => Challenge.SubmissionHistory;

        public  Challenge Challenge { get; }

        public IEnumerable<RuleEvaluation> RuleEvaluations
        {
            get => Evaluation.RuleEvaluations;
        }

        public ChallengeContext(Challenge challenge)
        {
            if (challenge is null)
            {
                throw new ArgumentNullException(nameof(challenge));
            }

            Evaluation = new ChallengeEvaluation();
            Challenge = challenge;
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
