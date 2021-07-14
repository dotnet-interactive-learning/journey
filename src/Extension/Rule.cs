using System;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;

namespace Extension
{
    public class Rule
    {
        private readonly Func<RuleContext, Task> evaluateRuleContextHandler;

        public Rule(Func<RuleContext, Task> ruleContraints)
        {
            this.evaluateRuleContextHandler = ruleContraints;
        }
        internal void TestResult(RuleContext result)
        {
            evaluateRuleContextHandler.Invoke(result);
        }
    }
}