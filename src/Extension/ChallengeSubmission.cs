using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;

namespace Extension
{
    public class ChallengeSubmission
    {
        public string SubmittedCode { get; private set; }
        public Evaluation Evaluation { get; private set; }
        public IEnumerable<KernelEvent> EventsProduced { get; private set; }
        public IEnumerable<Evaluation> RuleEvaluations => Evaluation.RuleEvaluations;

        public ChallengeSubmission(string submittedCode, Evaluation evaluation, IEnumerable<KernelEvent> events)
        {
            SubmittedCode = submittedCode;
            Evaluation = evaluation;
            EventsProduced = events;
        }
    }
}