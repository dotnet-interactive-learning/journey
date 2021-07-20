
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class LessonTests
    {
        private Challenge GetEmptyChallenge(string name = null)
        {
            return new Challenge(new EditableCode[] { }, name);
        }

        [Fact]
        public async Task starting_to_an_unrevealed_challenge_directly_reveals_it()
        {
            var lesson = new Lesson();
            var challenge = GetEmptyChallenge();

            await lesson.StartChallengeAsync(challenge);

            challenge.Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task starting_a_challenge_sets_the_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var challenge = GetEmptyChallenge();

            await lesson.StartChallengeAsync(challenge);

            lesson.CurrentChallenge.Should().Be(challenge);
        }

        [Fact]
        public async Task teacher_can_start_a_challenge_using_challenge_name()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetEmptyChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartChallengeAsync("3");
            });
            var challenge2 = GetEmptyChallenge("2");
            var challenge3 = GetEmptyChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge3);
        }

        [Fact]
        public async Task teacher_can_explicitly_proceed_to_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetEmptyChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            var challenge2 = GetEmptyChallenge("2");
            var challenge3 = GetEmptyChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task explicitly_proceeding_to_the_next_challenge_at_last_challenge_does_nothing()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetEmptyChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            lesson.AddChallenge(challenge1);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge1);
        }
    }
}
