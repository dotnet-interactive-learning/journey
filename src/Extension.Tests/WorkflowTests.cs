
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class WorkflowTests
    {
        private IReadOnlyList<EditableCode> sampleContent = new EditableCode[]
        {
            new EditableCode("markdown", @"
# Challenge 1

## Add 1 with 2 and return it
"),
            new EditableCode("csharp", @"
// write your answer here
")
        };

        private string sampleAnswer = @"
#!csharp
1 + 2
";

        private IReadOnlyList<EditableCode> sampleContent2 = new EditableCode[]
        {
            new EditableCode("markdown", @"
# Challenge 2

## Times 1 with 2 and return it
"),
            new EditableCode("csharp", @"
// write your answer here
")
        };

        [Fact(Skip = "wip")]
        public async Task Test()
        {
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            // teacher defines challenge
            var lesson = new Lesson();
            var challenge = new Challenge(sampleContent);
            var challenge2 = new Challenge(sampleContent2);
            challenge.AddRule(ruleContext =>
            {
                ruleContext.Fail();
            });
            challenge.OnCodeSubmittedAsync(async challengeContext =>
            {
                var numPassed = challengeContext.RuleEvaluations.Count(e => e.Passed);
                var total = challengeContext.RuleEvaluations.Count();
                if (numPassed / total >= 0.5)
                {
                    challengeContext.Pass("Good work! Challenge 1 passed.");
                    await challengeContext.StartChallengeAsync(challenge2);
                }
                else
                {
                    challengeContext.Fail("Keep working!");
                }
            });

            // teacher sends challenge
            await lesson.StartChallengeAsync(challenge);

            // student submit code
            await kernel.SubmitCodeAsync(sampleAnswer);

            kernel.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<DisplayedValueProduced>()
                .Which.FormattedValues
                .Should()
                .ContainSingle(v =>
                    v.MimeType == "text/html"
                    && v.Value.Contains("Failure: Incorrect solution."));
        }
    }
}
