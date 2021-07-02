using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using System;

namespace Extension
{
    public class ExecutionEvaluationProduced : KernelEvent
    {
        public ExecutionEvaluationProduced(
            SubmitCode command,
            Evaluation evaluation) : base(command)
        {
            if (evaluation is null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            Evaluation = evaluation;
        }

        public Evaluation Evaluation { get; }
    }
}