using Microsoft.DotNet.Interactive.Commands;

namespace Extension.Events
{
    public class QuestionTextEvaluationProduced : EvaluationProduced
    {
        public QuestionTextEvaluationProduced(
            SubmitCode command,
            Evaluation evaluation) : base(command, evaluation)
        {
        }
    }
}

