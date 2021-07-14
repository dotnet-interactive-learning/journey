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
    public class ChallengeGraphProgressionServiceTests
    {
        [Fact]
        public async Task challenge_without_dependency_is_autorevealed_in_the_single_challenge_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(1);

            await service.Commit();

            challenges[0].Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task challenge_without_dependency_is_set_as_current_in_the_single_challenge_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(1);

            await service.Commit();

            lesson.CurrentChallenge.Should().Be(challenges[0]);
        }

        [Fact]
        public async Task challenge_without_dependency_is_autorevealed_in_the_linear_caseAsync()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(3);

            service.UseLinearProgressionStructure();
            await service.Commit();

            challenges[0].Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task challenge_with_no_dependency_is_set_as_current_in_the_linear_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(3);

            service.UseLinearProgressionStructure();
            await service.Commit();

            lesson.CurrentChallenge.Should().Be(challenges[0]);
        }

        [Fact]
        public async Task passing_a_challenge_reveals_the_next_challenge_linear_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(3);
            service.UseLinearProgressionStructure();
            await service .Commit();

            service.Pass();
            await service.TryGoToNextChallengeAsync();

            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, true, false);

            service.Pass();
            await service.TryGoToNextChallengeAsync();

            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, true, true);
        }

        [Fact]
        public async Task skipping_to_a_challenge_sets_lesson_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(2);
            service.UseLinearProgressionStructure();
            await service .Commit();

            await service.ForceGoToChallengeAsync(challenges[1]);

            lesson.CurrentChallenge.Should().Be(challenges[1]);
        }

        [Fact]
        public async Task skipping_to_a_unrevealed_challenge_allows_progression_to_continue_from_there_linear_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(4);
            service.UseLinearProgressionStructure();
            await service.Commit();

            await service.ForceGoToChallengeAsync(challenges[2]);

            lesson.CurrentChallenge.Should().Be(challenges[2]);
            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, false, true, false);

            service.Pass();
            await service.TryGoToNextChallengeAsync();

            lesson.CurrentChallenge.Should().Be(challenges[3]);
            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, false, true, true);
        }

        [Fact]
        public async Task going_back_to_a_revealed_challenge_allows_progression_to_continue_to_progress_from_there_linear_case()
        {
            var lesson = new Lesson();
            var service = new ChallengeGraphProgressionService(lesson);
            var challenges = service.AddBlankChallenges(4);
            service.UseLinearProgressionStructure();
            await service.Commit();
            service.Pass();
            await service.TryGoToNextChallengeAsync();
            service.Pass();
            await service.TryGoToNextChallengeAsync();

            await service.ForceGoToChallengeAsync(challenges[1]);

            lesson.CurrentChallenge.Should().Be(challenges[1]);
            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, true, true, false);

            service.Pass();
            await service.TryGoToNextChallengeAsync();

            lesson.CurrentChallenge.Should().Be(challenges[2]);
            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, true, true, false);

            service.Pass();
            await service.TryGoToNextChallengeAsync();

            lesson.CurrentChallenge.Should().Be(challenges[3]);
            challenges.GetRevealedStatuses().Should().BeEquivalentTo(true, true, true, true);
        }
    }
}
