
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
            new EditableCode("markdown",
@"# Challenge 1

## Add 1 with 2 and return it"),

            new EditableCode("csharp",
@"// write your answer here")
        };

        private string sampleAnswer =
@"#!csharp
1 + 2";

        private IReadOnlyList<EditableCode> sampleContent2 = new EditableCode[]
        {
            new EditableCode("markdown",
@"# Challenge 2

## Times 1 with 2 and return it
"),

            new EditableCode("csharp", @"
// write your answer here
")
        };

        [Fact]
        public async Task Test()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);

            // teacher defines challenge
            var challenge1 = new Challenge(sampleContent, lesson);
            var challenge2 = new Challenge(sampleContent2, lesson);
            challenge1.AddRule(ruleContext =>
            {
                ruleContext.Fail("this rule failed because reasons");
            });
            challenge1.OnCodeSubmittedAsync(async challengeContext =>
            {
                var numPassed = challengeContext.RuleEvaluations.Count(e => e.Passed);
                var total = challengeContext.RuleEvaluations.Count();
                if (numPassed / total >= 0.5)
                {
                    challengeContext.SetMessage("Good work! Challenge 1 passed.");
                    await challengeContext.StartChallengeAsync(challenge2);
                }
                else
                {
                    challengeContext.SetMessage("Keep working!");
                }
            });

            // teacher sends challenge
            await lesson.StartChallengeAsync(challenge1);

            using var events = kernel.KernelEvents.ToSubscribedList();
            // student submit code
            await kernel.SubmitCodeAsync(sampleAnswer);

            events.Should().ContainSingle<DisplayedValueProduced>()
                .Which.FormattedValues
                .Should()
                .ContainSingle(v =>
                    v.MimeType == "text/html"
                    && v.Value.Contains("Failure:")
                    && v.Value.Contains("Keep working!")
                    && v.Value.Contains("this rule failed because reasons"));
        }


        [Fact]
        public async Task teacher_can_access_challenge_submission_history_for_challenge_evaluation()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);

            // teacher defines challenge
            var challenge1 = new Challenge(sampleContent, lesson);
            challenge1.AddRule(ruleContext =>
            {
                ruleContext.Fail("this rule failed because reasons");
            });
            challenge1.OnCodeSubmitted(challengeContext =>
            {
                var history = challengeContext.SubmissionHistory;
                var pastConsecFailures = 0;
                foreach (var submission in history)
                {
                    var numPassed = submission.RuleEvaluations.Count(e => e.Passed);
                    var total = submission.RuleEvaluations.Count();
                    if (numPassed / total < 0.5)
                    {
                        pastConsecFailures++;
                    }
                    else
                    {
                        pastConsecFailures = 0;
                    }
                }

                if (pastConsecFailures > 2)
                {
                    challengeContext.SetMessage("Enough! Try something else.");
                }
                else
                {
                    var numPassed = challengeContext.RuleEvaluations.Count(e => e.Passed);
                    var total = challengeContext.RuleEvaluations.Count();
                    if (numPassed / total >= 0.5)
                    {
                        challengeContext.SetMessage("Good work! Challenge 1 passed.");
                    }
                    else
                    {
                        challengeContext.SetMessage("Keep working!");
                    }
                }
            });

            // teacher sends challenge
            await lesson.StartChallengeAsync(challenge1);

            using var events = kernel.KernelEvents.ToSubscribedList();
            // student submit code
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);
            await kernel.SubmitCodeAsync(sampleAnswer);

            events
                .Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.FormattedValues.Single(
                        v => v.MimeType == "text/html").Value.Contains("Enough! Try something else."));
        }
    }
}
