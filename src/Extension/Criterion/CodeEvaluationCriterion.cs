namespace Extension.Criterion
{
    public class CodeEvaluationCriterion
    {
        string criterion;

        public static CodeEvaluationCriterion FromCodeString(string codeString)
        {
            return new CodeEvaluationCriterion(codeString);
        }

        private CodeEvaluationCriterion(string codeString)
        {
            criterion = codeString;
        }

        public string ToCodeString()
        {
            return criterion;
        }
    }
}