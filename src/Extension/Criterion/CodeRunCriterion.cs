namespace Extension.Criterion
{
    public class CodeRunCriterion
    {
        string criterion;

        public static CodeRunCriterion FromCodeString(string codeString)
        {
            return new CodeRunCriterion(codeString);
        }

        private CodeRunCriterion(string codeString)
        {
            criterion = codeString;
        }

        public string ToCodeString()
        {
            return criterion;
        }
    }
}