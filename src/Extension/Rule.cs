using System;
using Microsoft.DotNet.Interactive;

namespace Extension
{
    public class Rule
    {
        private readonly Action<Banana> evaluateBanana;

        public Rule(Action<Banana> ruleContraints)
        {
            this.evaluateBanana = ruleContraints;
        }
        internal void TestResult(Banana result)
        {
            evaluateBanana.Invoke(result);

           
        }
    }
}