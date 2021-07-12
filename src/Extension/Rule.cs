using System;
using Microsoft.DotNet.Interactive;

namespace Extension
{
    public class Rule
    {
        private readonly Func<KernelCommandResult, bool> ruleContraints;

        public Rule(Func<KernelCommandResult,bool> ruleContraints)
        {
            this.ruleContraints = ruleContraints;
        }
        internal void TestResult(KernelCommandResult result)
        {
            ruleContraints.Invoke(result);

            throw new NotImplementedException();
        }
    }
}