using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;

namespace Extension
{
    public class ChallengeSubmission
    {
        public string SubmissionCode { get; private set; }
        public ChallengeEvaluation Evaluation { get; private set; }
        public IEnumerable<KernelEvent> EventsProduced { get; private set; }
        public IEnumerable<RuleEvaluation> RuleEvaluations => Evaluation.RuleEvaluations;

        public ChallengeSubmission(string submissionCode, ChallengeEvaluation evaluation, IEnumerable<KernelEvent> events)
        {
            SubmissionCode = submissionCode;
            Evaluation = evaluation;
            EventsProduced = events;
        }
    }
}