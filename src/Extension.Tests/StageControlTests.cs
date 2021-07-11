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

        private Stage GetEmptyStage()
        {
            return new Stage(Enumerable.Empty<string>());
        }

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_single_stage_case()
        {
            var revealedStages = new List<Stage>();
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(1);
            stageController.AddOnRevealListeners(stage => revealedStages.Add(stage));

            stageController.Commit();

            stageController.CurrentStage.Should().Be(stages[0]);
            revealedStages.Should().BeEquivalentTo(stages[0]);
        }

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_multi_stage_linear_case()
        {
            var revealedStages = new List<Stage>();
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(3);
            stageController.AddOnRevealListeners(stage => revealedStages.Add(stage));

            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.CurrentStage.Should().Be(stages[0]);
            revealedStages.Should().BeEquivalentTo(stages[0]);
        }

        [Fact]
        public void passing_a_stage_reveals_the_next_stage_linear_case()
        {
            var revealedStages = new List<Stage>();
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(3);
            stageController.AddOnRevealListeners(stage => revealedStages.Add(stage));
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.PassStage();

            stageController.CurrentStage.Should().Be(stages[1]);
            revealedStages.Should().BeEquivalentTo(stages[0], stages[1]);

            stageController.PassStage();

            stageController.CurrentStage.Should().Be(stages[2]);
            revealedStages.Should().BeEquivalentTo(stages[0], stages[1], stages[2]);
        }

        [Fact]
        public void a_stage_exposes_itself_and_its_content_through_onReveal_listeners()
        {
            Stage exposedObject = null;
            var exposedContent = Enumerable.Empty<string>();
            Stage stage = new Stage(sampleStageContent);
            stage.AddOnRevealListener(stage =>
            {
                exposedObject = stage;
                exposedContent = stage.Contents;
            });

            stage.Reveal();

            exposedObject.Should().Be(stage);
            exposedContent.Should().BeEquivalentTo(sampleStageContent);
        }

        [Fact]
        public void a_stage_exposes_itself_and_its_content_through_onFocus_listeners()
        {
            Stage exposedObject = null;
            var exposedContent = Enumerable.Empty<string>();
            Stage stage = new Stage(sampleStageContent);
            stage.AddOnFocusListener(stage =>
            {
                exposedObject = stage;
                exposedContent = stage.Contents;
            });

            stage.Focus();

            exposedObject.Should().Be(stage);
            exposedContent.Should().BeEquivalentTo(sampleStageContent);
        }

        [Fact]
        public void calling_focus_on_a_unrevealed_stage_calls_onFocus_and_onReveal_listeners()
        {
            bool wasOnFocusCalled = false;
            bool wasOnRevealCalled = false;
            Stage stage = GetEmptyStage();
            stage.AddOnFocusListener(_ => wasOnFocusCalled = true);
            stage.AddOnRevealListener(_ => wasOnRevealCalled = true);

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
            stage.Reveal();

            numberOfListenerCalls.Should().Be(1);
        }


        [Fact]
        public void calling_focus_on_a_revealed_stage_calls_onFocus_but_not_onReveal_listeners()
        {
            int numberOfOnFocusListenerCalls = 0;
            int numberOfOnRevealListenerCalls = 0;
            var stage = GetEmptyStage();
            stage.Reveal();
            stage.AddOnFocusListener(_ => numberOfOnFocusListenerCalls++);
            stage.AddOnRevealListener(_ => numberOfOnRevealListenerCalls++);
            
            stage.Focus();

            numberOfOnFocusListenerCalls.Should().Be(1);
            numberOfOnRevealListenerCalls.Should().Be(0);
        }


        [Fact]
        public void skipping_to_a_unrevealed_stage_reveals_it()
        {
            bool didGetRevealed = false;
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(2);
            stages[1].AddOnRevealListener(_ => didGetRevealed = true);
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.GoToStage(stages[1]);

            stageController.CurrentStage.Should().Be(stages[1]);
            didGetRevealed.Should().BeTrue();
        }


        [Fact]
        public void skipping_to_a_unrevealed_stage_allows_progression_to_continue_from_there_linear_case()
        {
            var revealedStages = new List<Stage>();
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(4);
            stageController.AddOnRevealListeners(stage => revealedStages.Add(stage));
            stageController.UseLinearProgressionStructure();
            stageController.Commit();

            stageController.GoToStage(stages[2]);

            stageController.CurrentStage.Should().Be(stages[2]);
            revealedStages.Should().BeEquivalentTo(stages[0], stages[2]);

            stageController.PassStage();

            stageController.CurrentStage.Should().Be(stages[3]);
            revealedStages.Should().BeEquivalentTo(stages[0], stages[2], stages[3]);
        }

        [Fact]
        public void going_back_to_a_revealed_stage_allows_progression_to_continue_to_progress_from_there_linear_case()
        {
            var stageController = new StageController();
            var stages = stageController.AddBlankStages(4);
            stageController.UseLinearProgressionStructure();
            stageController.Commit();
            stageController.PassStage();
            stageController.PassStage();
            stageController.PassStage();

            stageController.GoToStage(stages[1]);

            stageController.CurrentStage.Should().Be(stages[1]);

            stageController.PassStage();

            stageController.CurrentStage.Should().Be(stages[2]);

            stageController.PassStage();

            stageController.CurrentStage.Should().Be(stages[3]);
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
