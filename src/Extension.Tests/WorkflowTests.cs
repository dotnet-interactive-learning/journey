using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class WorkflowTests : ProgressiveLearningTestBase
    {
        private IReadOnlyList<SendEditableCode> sampleContent = new SendEditableCode[]
        {
            new SendEditableCode("markdown",
@"# Challenge 1

## Add 1 with 2 and return it"),

            new SendEditableCode("csharp",
@"// write your answer here")
        };

        private string sampleAnswer =
@"#!csharp
1 + 2";

        private IReadOnlyList<SendEditableCode> sampleContent2 = new SendEditableCode[]
        {
            new SendEditableCode("markdown",
@"# Challenge 2

## Times 1 with 2 and return it
"),

            new SendEditableCode("csharp", @"
// write your answer here
")
        };

        [Fact]
        public async Task teacher_can_evaluate_a_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            using var events = kernel.KernelEvents.ToSubscribedList();

            // teacher defines challenge
            var challenge = new Challenge(contents: sampleContent);
            challenge.AddRule(ruleContext =>
            {
                ruleContext.Fail("this rule failed because reasons");
            });
            challenge.OnCodeSubmitted(challengeContext =>
            {
                var numPassed = challengeContext.RuleEvaluations.Count(e => e.Passed);
                var total = challengeContext.RuleEvaluations.Count();
                if (numPassed / total >= 0.5)
                { }
                else
                {
                    challengeContext.SetMessage("Keep working!");
                }
            });

            // teacher sends challenge
            await lesson.StartChallengeAsync(challenge);

            // student submit code
            await kernel.SubmitCodeAsync("1+1");

            events.Should().ContainSingle<DisplayedValueProduced>()
                .Which.FormattedValues
                .Should()
                .ContainSingle(v =>
                    v.MimeType == "text/html"
                    && v.Value.Contains("Keep working!")
                    && v.Value.Contains("this rule failed because reasons"));
        }

        [Fact]
        public async Task teacher_can_access_challenge_submission_history_for_challenge_evaluation()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            using var events = kernel.KernelEvents.ToSubscribedList();

            // teacher defines challenge
            var challenge = new Challenge(contents: sampleContent);
            challenge.AddRule(ruleContext =>
            {
                ruleContext.Fail("this rule failed because reasons");
            });
            challenge.OnCodeSubmitted(challengeContext =>
            {
                var numPassed = challengeContext.RuleEvaluations.Count(e => e.Passed);
                var total = challengeContext.RuleEvaluations.Count();
                if (numPassed / total >= 0.5)
                {
                    challengeContext.SetMessage("Good work! Challenge 1 passed.");
                }
                else
                {
                    var history = challengeContext.SubmissionHistory;
                    var pastFailures = 0;
                    foreach (var submission in history)
                    {
                        numPassed = submission.RuleEvaluations.Count(e => e.Passed);
                        total = submission.RuleEvaluations.Count();
                        if (numPassed / total < 0.5) pastFailures++;
                    }

                    if (pastFailures > 2)
                    {
                        challengeContext.SetMessage("Enough! Try something else.");
                    }
                    else
                    {
                        challengeContext.SetMessage("Keep working!");
                    }
                }
            });

            // teacher sends challenge
            await lesson.StartChallengeAsync(challenge);

            // student submit code
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);

            events
                .Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.Contains("Enough! Try something else."));
        }
    }
}
