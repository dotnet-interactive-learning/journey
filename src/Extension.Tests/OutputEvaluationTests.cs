using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Xunit;

namespace Extension.Tests
{
    public class OutputEvaluationTests : ProgressiveLearningTestBase
    {
        [Fact]
        public async Task teacher_can_provide_challenge_evaluation_feedback()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge = GetEmptyChallenge();
            challenge.OnCodeSubmitted(context =>
            {
                context.SetMessage("123", 3);
            });
            await lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("1 + 1");

            challenge.CurrentEvaluation.Message.Should().Be("123");
            challenge.CurrentEvaluation.Hint.Should().Be(3);
        }

        [Fact]
        public async Task teacher_can_fail_rule_evaluation_and_provide_feedback_and_hint()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge = GetEmptyChallenge();
            challenge.AddRule(context => context.Fail("abc", 3));
            await lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("1 + 1");

            challenge.CurrentEvaluation.RuleEvaluations.Single().Outcome.Should().Be(Outcome.Failure);
            challenge.CurrentEvaluation.RuleEvaluations.Single().Reason.Should().Be("abc");
            challenge.CurrentEvaluation.RuleEvaluations.Single().Hint.Should().Be(3);
        }

        [Fact]
        public async Task teacher_can_pass_rule_evaluation_and_provide_feedback_and_hint()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge = GetEmptyChallenge();
            challenge.AddRule(context => context.Pass("abc", 3));
            await lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("1 + 1");

            challenge.CurrentEvaluation.RuleEvaluations.Single().Outcome.Should().Be(Outcome.Success);
            challenge.CurrentEvaluation.RuleEvaluations.Single().Reason.Should().Be("abc");
            challenge.CurrentEvaluation.RuleEvaluations.Single().Hint.Should().Be(3);
        }

        [Fact]
        public async Task teacher_can_partially_pass_rule_evaluation_and_provide_feedback_and_hint()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge = GetEmptyChallenge();
            challenge.AddRule(context => context.PartialPass("abc", 3));
            await lesson.StartChallengeAsync(challenge);


            await kernel.SubmitCodeAsync("1 + 1");

            challenge.CurrentEvaluation.RuleEvaluations.Single().Outcome.Should().Be(Outcome.PartialSuccess);
            challenge.CurrentEvaluation.RuleEvaluations.Single().Reason.Should().Be("abc");
            challenge.CurrentEvaluation.RuleEvaluations.Single().Hint.Should().Be(3);
        }
    }
}
