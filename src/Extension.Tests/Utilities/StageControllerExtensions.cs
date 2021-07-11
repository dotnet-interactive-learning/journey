using Extension.StageControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class StageControllerExtensions
    {
        public static List<Stage> AddBlankStages(this StageController stageController, int numberOfStages)
        {
            var stages = new List<Stage>();

            for (int i = 0; i < numberOfStages; i++)
            {
                var stage = new Stage(Enumerable.Empty<string>());
                stages.Add(stage);
                stageController.AddStage(stage);
            }

            return stages;
        }
    }
}
