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
    public class ChallengeTests
    {
        private string[] sampleChallengeContent = new string[]
            {
                "contentcell1",
                "contentcell2",
                "contentcell3"
            };

        private Challenge GetEmptyChallenge()
        {
            return new Challenge(Enumerable.Empty<string>());
        }

        [Fact]
        public void challenge_with_no_dependency_is_auto_revealed_single_challenge_case()
        {
            var revealedChallenges = new List<Challenge>();
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(1);
            challengeController.AddOnRevealListeners(challenge => revealedChallenges.Add(challenge));

            challengeController.Commit();

            challengeController.CurrentChallenge.Should().Be(challenges[0]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0]);
        }

        [Fact]
        public void challenge_with_no_dependency_is_auto_revealed_multi_challenge_linear_case()
        {
            var revealedChallenges = new List<Challenge>();
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(3);
            challengeController.AddOnRevealListeners(challenge => revealedChallenges.Add(challenge));

            challengeController.UseLinearProgressionStructure();
            challengeController.Commit();

            challengeController.CurrentChallenge.Should().Be(challenges[0]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0]);
        }

        [Fact]
        public void passing_a_challenge_reveals_the_next_challenge_linear_case()
        {
            var revealedChallenges = new List<Challenge>();
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(3);
            challengeController.AddOnRevealListeners(challenge => revealedChallenges.Add(challenge));
            challengeController.UseLinearProgressionStructure();
            challengeController.Commit();

            challengeController.PassChallenge();

            challengeController.CurrentChallenge.Should().Be(challenges[1]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0], challenges[1]);

            challengeController.PassChallenge();

            challengeController.CurrentChallenge.Should().Be(challenges[2]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0], challenges[1], challenges[2]);
        }

        [Fact]
        public void a_challenge_exposes_itself_and_its_content_through_onReveal_listeners()
        {
            Challenge exposedObject = null;
            var exposedContent = Enumerable.Empty<string>();
            Challenge challenge = new Challenge(sampleChallengeContent);
            challenge.AddOnRevealListener(challenge =>
            {
                exposedObject = challenge;
                exposedContent = challenge.Contents;
            });

            challenge.Reveal();

            exposedObject.Should().Be(challenge);
            exposedContent.Should().BeEquivalentTo(sampleChallengeContent);
        }

        [Fact]
        public void a_challenge_exposes_itself_and_its_content_through_onFocus_listeners()
        {
            Challenge exposedObject = null;
            var exposedContent = Enumerable.Empty<string>();
            Challenge challenge = new Challenge(sampleChallengeContent);
            challenge.AddOnFocusListener(challenge =>
            {
                exposedObject = challenge;
                exposedContent = challenge.Contents;
            });

            challenge.Focus();

            exposedObject.Should().Be(challenge);
            exposedContent.Should().BeEquivalentTo(sampleChallengeContent);
        }

        [Fact]
        public void calling_focus_on_a_unrevealed_challenge_calls_onFocus_and_onReveal_listeners()
        {
            bool wasOnFocusCalled = false;
            bool wasOnRevealCalled = false;
            Challenge challenge = GetEmptyChallenge();
            challenge.AddOnFocusListener(_ => wasOnFocusCalled = true);
            challenge.AddOnRevealListener(_ => wasOnRevealCalled = true);

            challenge.Focus();

            wasOnFocusCalled.Should().BeTrue();
            wasOnRevealCalled.Should().BeTrue();
        }

        [Fact]
        public void calling_reveal_on_a_challenge_that_is_already_revealed_does_not_call_listener()
        {
            int numberOfListenerCalls = 0;
            var challenge = GetEmptyChallenge();
            challenge.AddOnRevealListener(_ => numberOfListenerCalls++);

            challenge.Reveal();
            challenge.Reveal();

            numberOfListenerCalls.Should().Be(1);
        }


        [Fact]
        public void calling_focus_on_a_revealed_challenge_calls_onFocus_but_not_onReveal_listeners()
        {
            int numberOfOnFocusListenerCalls = 0;
            int numberOfOnRevealListenerCalls = 0;
            var challenge = GetEmptyChallenge();
            challenge.Reveal();
            challenge.AddOnFocusListener(_ => numberOfOnFocusListenerCalls++);
            challenge.AddOnRevealListener(_ => numberOfOnRevealListenerCalls++);
            
            challenge.Focus();

            numberOfOnFocusListenerCalls.Should().Be(1);
            numberOfOnRevealListenerCalls.Should().Be(0);
        }


        [Fact]
        public void skipping_to_a_unrevealed_challenge_reveals_it()
        {
            bool didGetRevealed = false;
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(2);
            challenges[1].AddOnRevealListener(_ => didGetRevealed = true);
            challengeController.UseLinearProgressionStructure();
            challengeController.Commit();

            challengeController.GoToChallenge(challenges[1]);

            challengeController.CurrentChallenge.Should().Be(challenges[1]);
            didGetRevealed.Should().BeTrue();
        }


        [Fact]
        public void skipping_to_a_unrevealed_challenge_allows_progression_to_continue_from_there_linear_case()
        {
            var revealedChallenges = new List<Challenge>();
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(4);
            challengeController.AddOnRevealListeners(challenge => revealedChallenges.Add(challenge));
            challengeController.UseLinearProgressionStructure();
            challengeController.Commit();

            challengeController.GoToChallenge(challenges[2]);

            challengeController.CurrentChallenge.Should().Be(challenges[2]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0], challenges[2]);

            challengeController.PassChallenge();

            challengeController.CurrentChallenge.Should().Be(challenges[3]);
            revealedChallenges.Should().BeEquivalentTo(challenges[0], challenges[2], challenges[3]);
        }

        [Fact]
        public void going_back_to_a_revealed_challenge_allows_progression_to_continue_to_progress_from_there_linear_case()
        {
            var challengeController = new ChallengeController();
            var challenges = challengeController.AddBlankChallenges(4);
            challengeController.UseLinearProgressionStructure();
            challengeController.Commit();
            challengeController.PassChallenge();
            challengeController.PassChallenge();
            challengeController.PassChallenge();

            challengeController.GoToChallenge(challenges[1]);

            challengeController.CurrentChallenge.Should().Be(challenges[1]);

            challengeController.PassChallenge();

            challengeController.CurrentChallenge.Should().Be(challenges[2]);

            challengeController.PassChallenge();

            challengeController.CurrentChallenge.Should().Be(challenges[3]);
        }


        [Fact]
        public void assigning_a_challenge_directly_to_CurrentChallenge_focuses_on_it()
        {
            bool didGetFocus = false;
            var challengeController = new ChallengeController();
            var someChallenge = GetEmptyChallenge();
            someChallenge.AddOnFocusListener(_ => didGetFocus = true);

            challengeController.CurrentChallenge = someChallenge;

            didGetFocus.Should().BeTrue();
        }
    }
}
