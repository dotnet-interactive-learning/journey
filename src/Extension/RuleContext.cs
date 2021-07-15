namespace Extension
{
    public class RuleContext
    {
        public bool Passed { get;private set; }

        public void Fail()
        {
            Passed = false;

        }

        public void Pass()
        {
            Passed = true;
        }
    }

}
