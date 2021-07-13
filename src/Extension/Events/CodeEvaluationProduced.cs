using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Extension.Events
{
    public class CodeEvaluationProduced : KernelEvent
    {
        public Evaluation Evaluation { get; }

        public CodeEvaluationProduced(
            SubmitCode command,
            Evaluation evaluation) : base(command)
        {
            Evaluation = evaluation;
        }
    }
}