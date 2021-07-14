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
    public class ChallengeTests
    {
        private Challenge GetEmptyChallenge(Lesson lesson = null)
        {
            return new Challenge(new EditableCode[] { }, lesson);
        }

        [Fact]
        public async Task can_use_on_code_submitted_handler_to_skip_to_a_specific_challenge()
        {
            var lesson = new Lesson();
            var challenge1 = GetEmptyChallenge(lesson);
            var challenge2 = GetEmptyChallenge(lesson);
            challenge1.OnCodeSubmittedAsync(async (context) =>
            {
                await context.StartChallengeAsync(challenge2);
            });

            await challenge1.InvokeOnEvaluationComplete();

            lesson.CurrentChallenge.Should().Be(challenge2);
        }
    }
}
