using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;

namespace Microsoft.DotNet.Interactive.Journey
{
    public class ChallengeSubmission
    {
        public string SubmittedCode { get; }
        public ChallengeEvaluation Evaluation { get; }
        public IEnumerable<KernelEvent> EventsProduced { get; }
        public IEnumerable<RuleEvaluation> RuleEvaluations => Evaluation.RuleEvaluations;

        public ChallengeSubmission(string submittedCode, ChallengeEvaluation evaluation, IEnumerable<KernelEvent> events)
        {
            SubmittedCode = submittedCode;
            Evaluation = evaluation;
            EventsProduced = events;
        }
    }
}