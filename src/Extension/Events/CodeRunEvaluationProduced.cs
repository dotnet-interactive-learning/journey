using Microsoft.DotNet.Interactive.Commands;

namespace Extension.Events
{
    public class CodeRunEvaluationProduced : EvaluationProduced
    {
        public CodeRunEvaluationProduced(
            SubmitCode command,
            Evaluation evaluation) : base(command, evaluation)
        {
        }
    }
}