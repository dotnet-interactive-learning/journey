using System;

namespace Extension
{
    public class RuleContext
    {
        public bool Passed { get;private set; }

        public void Fail()
        {
            this.Passed = false;

            
        }

        public void Pass()
        {
            this.Passed = true;


        }
    }

}