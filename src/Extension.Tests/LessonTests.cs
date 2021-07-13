using Extension.ChallengeControl;
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
        [Fact]
        public void assigning_a_challenge_directly_calls_focus_on_it()
        {
            var onFocusGetsCalled = false;
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3");
            lesson.ChallengeController.Commit();
            challenges["3"].AddOnFocusListener(_ => onFocusGetsCalled = true);

            lesson.CurrentChallenge = challenges["3"];

            onFocusGetsCalled.Should().BeTrue();
        }

        [Fact]
        public void skipping_to_a_specific_challenge_calls_focus_on_it()
        {
            var onFocusGetsCalled = false;
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3");
            lesson.ChallengeController.Commit();
            challenges["3"].AddOnFocusListener(_ => onFocusGetsCalled = true);

            lesson.GoToChallenge("3");

            onFocusGetsCalled.Should().BeTrue();
        }

        [Fact]
        public void skipping_to_a_specific_challenge_sets_the_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3");
            lesson.ChallengeController.Commit();

            lesson.GoToChallenge("3");

            lesson.CurrentChallenge.Should().Be(challenges["3"]);
        }

        [Fact]
        public void can_use_on_evaluation_complete_handler_to_skip_to_a_specific_challenge()
        {
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3");
            lesson.ChallengeController.Commit();
            challenges["1"].OnEvaluationComplete((challenge, lesson) =>
            {
                lesson.GoToChallenge("3");
            });

            challenges["1"].InvokeOnEvaluationComplete();

            lesson.CurrentChallenge.Should().Be(challenges["3"]);
        }

        [Fact]
        public void linear_case_challenges_progress_according_to_insertion_order()
        {
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3");
            lesson.ChallengeController.UseLinearProgressionStructure();

            lesson.ChallengeController.Commit();

            lesson.CurrentChallenge.Should().Be(challenges["1"]);

            lesson.CurrentChallenge.Pass();

            lesson.CurrentChallenge.Should().Be(challenges["2"]);

            lesson.CurrentChallenge.Pass();

            lesson.CurrentChallenge.Should().Be(challenges["3"]);
        }

        [Fact]
        public void skipping_to_a_unrevealed_challenge_allows_progression_to_continue_from_there_linear_case()
        {
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3", "4");
            lesson.ChallengeController.UseLinearProgressionStructure();
            lesson.ChallengeController.Commit();

            lesson.GoToChallenge("3");

            lesson.CurrentChallenge.Should().Be(challenges["3"]);

            lesson.CurrentChallenge.Pass();

            lesson.CurrentChallenge.Should().Be(challenges["4"]);
        }

        [Fact]
        public void going_back_to_a_revealed_challenge_allows_progression_to_continue_to_progress_from_there_linear_case()
        {
            var lesson = new Lesson();
            var challenges = lesson.AddBlankChallenges("1", "2", "3", "4");
            lesson.ChallengeController.UseLinearProgressionStructure();
            lesson.ChallengeController.Commit();
            lesson.CurrentChallenge.Pass();
            lesson.CurrentChallenge.Pass();
            lesson.CurrentChallenge.Pass();

            lesson.GoToChallenge("2");

            lesson.CurrentChallenge.Should().Be(challenges["2"]);

            lesson.CurrentChallenge.Pass();

            lesson.CurrentChallenge.Should().Be(challenges["3"]);

            lesson.CurrentChallenge.Pass();

            lesson.CurrentChallenge.Should().Be(challenges["4"]);
        }
    }
}
