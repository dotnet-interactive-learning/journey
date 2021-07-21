
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class LessonTests : ProgressiveLearningTestBase
    {
        private Challenge GetChallenge(string name = null)
        {
            return new Challenge(name: name);
        }

        private List<SendEditableCode> GetSendEditableCode(string code)
        {
            return new List<SendEditableCode>
            {
                new SendEditableCode("csharp", code)
            };
        }

        [Fact]
        public async Task starting_to_an_unrevealed_challenge_directly_reveals_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();
            lesson.AddChallenge(challenge);

            await lesson.StartLessonAsync();

            challenge.Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task starting_a_challenge_sets_the_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();
            lesson.AddChallenge(challenge);

            await lesson.StartLessonAsync();

            lesson.CurrentChallenge.Should().Be(challenge);
        }

        [Fact]
        public async Task teacher_can_start_a_challenge_using_challenge_name()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge1 = new Challenge(GetSendEditableCode("1"), name: "1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartChallengeAsync("3");
            });
            var challenge2 = new Challenge(GetSendEditableCode("2"), name: "2");
            var challenge3 = new Challenge(GetSendEditableCode("3"), name: "3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartLessonAsync();

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge3);
        }

        [Fact]
        public async Task teacher_can_explicitly_start_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartLessonAsync();

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task explicitly_starting_the_next_challenge_at_last_challenge_does_nothing()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            lesson.AddChallenge(challenge1);
            await lesson.StartLessonAsync();

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(null);
        }

        [Fact]
        public async Task when_a_student_submits_code_to_a_challenge_they_move_to_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge1 = GetChallenge("1");
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartLessonAsync();

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task when_a_student_completes_the_last_challenge_then_the_lesson_is_completed()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge1 = GetChallenge("1");
            var challenge2 = GetChallenge("2");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            await lesson.StartLessonAsync();

            await kernel.SubmitCodeAsync("1+1");
            await kernel.SubmitCodeAsync("2+1");

            lesson.CurrentChallenge.Should().Be(null);
        }
    }
}
