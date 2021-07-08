using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.StageControl
{
    public class StageController
    {
        private SortedDictionary<int, Stage> Stages = new SortedDictionary<int, Stage>();
        private Dictionary<string, int> questionIdToStageIdMap = new Dictionary<string, int>();

        public StageController()
        {

        }

        public void AddStage(int stageId, string questionId, IEnumerable<string> content)
        {
            Stages.Add(stageId, new Stage(stageId, content));
            questionIdToStageIdMap.Add(questionId, stageId);
        }

        public void AddOnRevealListener(Action<Stage> listener)
        {
            foreach (var stage in Stages.Values)
            {
                stage.AddOnRevealListener(listener);
            }
        }

        public void PassStage(string questionId)
        {
            var stage = Stages[questionIdToStageIdMap[questionId]];
            stage.Pass();
        }

        public void UseLinearProgressionStructure()
        {
            ClearDependencyRelationships();
            var stages = new List<Stage>(Stages.Values);

            if (stages.Count >= 2)
            {
                var prevStage = stages[0];
                for (int i = 1; i < stages.Count; i++)
                {
                    var currStage = stages[i];
                    currStage.AddDependency(prevStage);
                    prevStage.AddDependent(currStage);
                    prevStage = currStage;
                }
            }

            CommitProgressionStructure();
        }

        public void CommitProgressionStructure()
        {
            // check on dependency graph validity
            RevealStartingStages();
        }

        private void RevealStartingStages()
        {
            var stagesWithNoDependencies = Stages.Values.Where(stage => stage.Dependencies.Count == 0);
            foreach (var stage in stagesWithNoDependencies)
            {
                if (!stage.Revealed)
                    stage.Reveal();
            }
        }

        private void ClearDependencyRelationships()
        {
            foreach (var stage in Stages.Values)
            {
                stage.ClearDependencyRelationships();
            }
        }
    }
}
