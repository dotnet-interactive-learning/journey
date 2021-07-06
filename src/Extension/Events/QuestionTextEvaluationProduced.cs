using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Extension.Events
{
    public class QuestionTextEvaluationProduced : KernelEvent
    {
        public Evaluation Evaluation { get; }

        public QuestionTextEvaluationProduced(
            SubmitCode command,
            Evaluation evaluation) : base(command)
        {
            Evaluation = evaluation;
        }
    }
}

