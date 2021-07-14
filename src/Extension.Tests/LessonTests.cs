
using Extension.Tests.Utilities;
using FluentAssertions;
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
        private Challenge GetEmptyChallenge()
        {
            return new Challenge(new EditableCode[] { });
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
    }
}
