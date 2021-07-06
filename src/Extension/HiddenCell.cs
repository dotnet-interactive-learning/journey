namespace Extension
{
    public class HiddenCell
    {
        private string code;

        public static HiddenCell FromCode(string code)
        {
            return new HiddenCell(code);
        }

        private HiddenCell(string code)
        {
            this.code = code;
        }

        public string ToCode()
        {
            return code;
        }
    }
}