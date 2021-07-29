using System;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;

namespace Extension
{
    public class Rule
    {
        public string Name { get; }
        private readonly Func<RuleContext, Task> evaluateRuleContextHandler;

        public Rule(Func<RuleContext, Task> ruleContraints, string name = null)
        {
            Name = name;
            evaluateRuleContextHandler = ruleContraints;
        }
        internal async Task Evaluate(RuleContext context)
        {
            await evaluateRuleContextHandler.Invoke(context);
        }
    }
}