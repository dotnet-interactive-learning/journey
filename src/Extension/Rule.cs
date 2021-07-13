using System;
using Microsoft.DotNet.Interactive;

namespace Extension
{
    public class Rule
    {
        private readonly Action<RuleContext> evaluateRuleContextHandler;

        public Rule(Action<RuleContext> ruleContraints)
        {
            this.evaluateRuleContextHandler = ruleContraints;
        }
        internal void TestResult(RuleContext result)
        {
            evaluateRuleContextHandler.Invoke(result);

           
        }
    }
}