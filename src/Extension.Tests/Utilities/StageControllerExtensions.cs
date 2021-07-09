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
        public static void AddBlankStages(this StageController stageController, params int[] stageIds)
        {
            foreach (var stageId in stageIds)
            {
                stageController.AddStage(stageId, Enumerable.Empty<string>());
            }
        }
    }
}
