using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.StageControl
{
    public class StageController
    {
        public SortedDictionary<int, Stage> Stages { get; private set; } = new SortedDictionary<int, Stage>();
        public Stage CurrentStage { get; private set; }

        public StageController()
        {

        }

        public void AddStage(int stageId, IEnumerable<string> content)
        {
            Stages.Add(stageId, new Stage(stageId, content));
        }

        public void AddOnRevealListener(Action<Stage> listener)
        {
            foreach (var stage in Stages.Values)
            {
                stage.AddOnRevealListener(listener);
            }
        }

        public void PassStage()
        {
            CurrentStage.Pass();
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

            Commit();
        }

        public void Commit()
        {
            InitializeStartingStages();
            foreach (var stage in Stages.Values)
            {
                stage.AddOnRevealListener(stage => CurrentStage = stage);
            }
        }

        private void InitializeStartingStages()
        {
            var stagesWithNoDependencies = Stages.Values.Where(stage => stage.Dependencies.Count == 0);
            CurrentStage = stagesWithNoDependencies.First();
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
