using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class ChallengeTests
    {
        private Challenge GetEmptyChallenge(Lesson lesson = null)
        {
            return new Challenge(new EditableCode[] { }, lesson);
        }

        // todo:  this test should be changed to use end to end, this is prob too artificial
        [Fact]
        public async Task teacher_can_skip_to_a_specific_challenge_when_evaluating_a_challenge()
        {
            var lesson = new Lesson();
            var challenge1 = GetEmptyChallenge(lesson);
            var challenge2 = GetEmptyChallenge(lesson);
            challenge1.OnCodeSubmittedAsync(async (context) =>
            {
                await context.StartChallengeAsync(challenge2);
            });
            await challenge1.Evaluate();

            await challenge1.InvokeOnCodeSubmittedHandler();

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task teacher_can_access_code_from_submission_history_when_evaluating_a_challenge()
        {
            var capturedCode = new List<string>();
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge = GetEmptyChallenge(lesson);
            await lesson.StartChallengeAsync(challenge);
            challenge.OnCodeSubmitted(context =>
            {
                capturedCode = context.SubmissionHistory.Select(h => h.SubmittedCode).ToList();
            });

            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 2");
            await kernel.SubmitCodeAsync("");

            challenge.SubmissionHistory.Select(h => h.SubmittedCode).ToList().Should().BeEquivalentTo("","1 + 2", "1 + 1");
            capturedCode.Should().BeEquivalentTo("1 + 2", "1 + 1");
        }

        [Fact]
        public async Task teacher_can_access_events_from_submission_history_when_evaluating_a_challenge()
        {
            var capturedEvents = new List<List<KernelEvent>>();
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge = GetEmptyChallenge(lesson);
            await lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("alsjl");
            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 2");
            capturedEvents = challenge.SubmissionHistory.Select(s => s.EventsProduced.ToList()).ToList();
            capturedEvents.Should().SatisfyRespectively(new Action<List<KernelEvent>>[]
            {
                evts => evts.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(3),
                evts => evts.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(2),
                evts => evts.Should().ContainSingle<CommandFailed>()
            });
        }

        [Fact]
        public async Task teacher_can_access_evaluations_from_submission_history_when_evaluating_a_challenge()
        {
            var capturedEvaluation = new List<ChallengeEvaluation>();
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge = GetEmptyChallenge(lesson);
            await lesson.StartChallengeAsync(challenge);
            bool isFirstSubmission = true;
            challenge.OnCodeSubmitted(context =>
            {
                if (isFirstSubmission)
                {
                    context.SetMessage("1st");
                    isFirstSubmission = false;
                }
                else
                {
                    context.SetMessage("not 1st");
                }
            });

            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 1");

            capturedEvaluation = challenge.SubmissionHistory.Select(h => h.Evaluation).ToList();

            capturedEvaluation.Should().SatisfyRespectively(new Action<ChallengeEvaluation>[]
            {
                e => e.Message.Should().Be("not 1st"),
                e => e.Message.Should().Be("not 1st"),
                e => e.Message.Should().Be("1st")
            });
        }

        [Fact]
        public async Task teacher_can_access_code_when_evaluating_a_rule()
        {
            var capturedCode = new List<string>();
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge = GetEmptyChallenge(lesson);
            await lesson.StartChallengeAsync(challenge);
            challenge.AddRule(context =>
            {
                capturedCode.Add(context.SubmittedCode);
            });

            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 2");
            await kernel.SubmitCodeAsync("1 + 3");

            capturedCode.Should().BeEquivalentTo("1 + 1", "1 + 2", "1 + 3");
        }

        [Fact]
        public async Task teacher_can_access_events_when_evaluating_a_rule()
        {
            var capturedEvents = new List<List<KernelEvent>>();
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge = GetEmptyChallenge(lesson);
            await lesson.StartChallengeAsync(challenge);
            challenge.AddRule(context =>
            {
                capturedEvents.Add(context.EventsProduced.ToList());
            });

            await kernel.SubmitCodeAsync("alsjkdf");
            await kernel.SubmitCodeAsync("1 + 1");
            await kernel.SubmitCodeAsync("1 + 2");

            capturedEvents.Should().SatisfyRespectively(new Action<List<KernelEvent>>[]
            {
                evts => evts.Should().ContainSingle<CommandFailed>(),
                evts => evts.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(2),
                evts => evts.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(3)
            });
        }
    }
}
