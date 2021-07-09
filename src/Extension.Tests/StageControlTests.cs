using Extension.StageControl;
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

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_single_stage_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddStage(1, Enumerable.Empty<string>());
            stageController.AddOnRevealListener(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.Commit();

            revealedStageIds.Should().BeEquivalentTo(1);
        }

        [Fact]
        public void stage_with_no_dependency_is_auto_revealed_multi_stage_linear_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddStage(1, Enumerable.Empty<string>());
            stageController.AddStage(2, Enumerable.Empty<string>());
            stageController.AddStage(3, Enumerable.Empty<string>());
            stageController.AddOnRevealListener(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.UseLinearProgressionStructure();

            revealedStageIds.Should().BeEquivalentTo(1);
        }

        [Fact]
        public void passing_a_stage_reveals_the_next_stage_linear_case()
        {
            List<int> revealedStageIds = new List<int>();

            var stageController = new StageController();
            stageController.AddStage(1, Enumerable.Empty<string>());
            stageController.AddStage(2, Enumerable.Empty<string>());
            stageController.AddStage(3, Enumerable.Empty<string>());
            stageController.AddOnRevealListener(stage =>
            {
                revealedStageIds.Add(stage.StageId);
            });
            stageController.UseLinearProgressionStructure();

            revealedStageIds.Should().BeEquivalentTo(1);

            stageController.PassStage();
            revealedStageIds.Should().BeEquivalentTo(1, 2);

            stageController.PassStage();
            revealedStageIds.Should().BeEquivalentTo(1, 2, 3);

            stageController.PassStage();
            revealedStageIds.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void revealing_a_stage_exposes_its_contents_through_listener()
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
        public void revealing_a_stage_that_is_already_revealed_does_not_call_listener()
        {
            int numberOfListenerCalls = 0;

            var stage = new Stage(1, Enumerable.Empty<string>());
            stage.AddOnRevealListener(_ => numberOfListenerCalls++);

            stage.Reveal();
            numberOfListenerCalls.Should().Be(1);

            try
            {
                stage.Reveal();
            }
            catch (Exception)
            {
            }
            numberOfListenerCalls.Should().Be(1);
        }
    }
}
