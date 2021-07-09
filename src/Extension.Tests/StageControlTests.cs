using Extension.StageControl;
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
    public class StageControlTests
    {
        private string[] sampleStageContent = new string[]
            {
                "contentcell1",
                "contentcell2",
                "contentcell3"
            };

        private Stage GetEmptyStage(int stageId = 1)
        {
            return new Stage(stageId, Enumerable.Empty<string>());
        }

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_single_stage_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddBlankStages(1);
            stageController.AddOnRevealListeners(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.Commit();

            stageController.CurrentStage.StageId.Should().Be(1);
            revealedStageIds.Should().BeEquivalentTo(1);
        }

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_multi_stage_linear_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddBlankStages(1, 2, 3);
            stageController.AddOnRevealListeners(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.CurrentStage.StageId.Should().Be(1);
            revealedStageIds.Should().BeEquivalentTo(1);
        }

        [Fact]
        public void passing_a_stage_reveals_the_next_stage_linear_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddBlankStages(1, 2, 3);
            stageController.AddOnRevealListeners(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.PassStage();
            stageController.CurrentStage.StageId.Should().Be(2);
            revealedStageIds.Should().BeEquivalentTo(1, 2);

            stageController.PassStage();
            stageController.CurrentStage.StageId.Should().Be(3);
            revealedStageIds.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void revealing_a_stage_exposes_its_contents_through_onReveal_listener()
        {
            IEnumerable<string> exposedContents = Enumerable.Empty<string>();

            Stage stage = new Stage(1, sampleStageContent);

            stage.AddOnRevealListener(stage =>
            {
                exposedContents = stage.Contents;
            });

            stage.Reveal();

            exposedContents.Should().BeEquivalentTo(sampleStageContent);
        }

        [Fact]
        public void focusing_on_a_unrevealed_stage_calls_onFocus_and_onReveal_listeners()
        {
            bool wasOnFocusCalled = false;
            bool wasOnRevealCalled = false;

            Stage stage = new Stage(1, sampleStageContent);

            stage.AddOnFocusListener(stage =>
            {
                wasOnFocusCalled = true;
            });
            stage.AddOnRevealListener(stage =>
            {
                wasOnRevealCalled = true;
            });

            stage.Focus();

            wasOnFocusCalled.Should().BeTrue();
            wasOnRevealCalled.Should().BeTrue();
        }

        [Fact]
        public void calling_reveal_on_a_stage_that_is_already_revealed_does_not_call_listener()
        {
            int numberOfListenerCalls = 0;

            var stage = GetEmptyStage();
            stage.AddOnRevealListener(_ => numberOfListenerCalls++);

            stage.Reveal();
            numberOfListenerCalls.Should().Be(1);


            stage.Reveal();
            numberOfListenerCalls.Should().Be(1);
        }


        [Fact]
        public void skipping_to_a_unrevealed_stage_reveals_it()
        {
            bool didGetRevealed = false;

            var stageController = new StageController();
            stageController.AddBlankStages(1, 2);
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.Stages[2].AddOnRevealListener(_ => didGetRevealed = true);

            stageController.GoToStage(2);

            stageController.CurrentStage.StageId.Should().Be(2);
            didGetRevealed.Should().BeTrue();
        }


        [Fact]
        public void skipping_to_a_unrevealed_stage_allows_progression_to_continue_from_there_linear_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddBlankStages(1, 2, 3, 4);
            stageController.AddOnRevealListeners(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.GoToStage(3);
            stageController.CurrentStage.StageId.Should().Be(3);
            revealedStageIds.Should().BeEquivalentTo(1, 3);

            stageController.PassStage();
            stageController.CurrentStage.StageId.Should().Be(4);
            revealedStageIds.Should().BeEquivalentTo(1, 3, 4);
        }

        [Fact]
        public void going_back_to_a_revealed_stage_allows_progression_to_continue_to_progress_from_there_linear_case()
        {
            var stageController = new StageController();
            stageController.AddBlankStages(1, 2, 3, 4);
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.PassStage();
            stageController.PassStage();
            stageController.PassStage();

            stageController.GoToStage(2);
            stageController.CurrentStage.StageId.Should().Be(2);

            stageController.PassStage();
            stageController.CurrentStage.StageId.Should().Be(3);

            stageController.PassStage();
            stageController.CurrentStage.StageId.Should().Be(4);
        }


        [Fact]
        public void assigning_a_stage_directly_to_CurrentStage_focuses_on_it()
        {
            bool didGetFocus = false;

            var stageController = new StageController();

            var someStage = GetEmptyStage();
            someStage.AddOnFocusListener(_ => didGetFocus = true);

            stageController.CurrentStage = someStage;

            didGetFocus.Should().BeTrue();
        }
    }
}
