using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.StageControl
{
    public class StageController
    {
        public HashSet<Stage> Stages { get; set; } = new HashSet<Stage>();
        public Stage CurrentStage
        {
            get
            {
                return _currentStage;
            }
            set
            {
                _currentStage = value;
                _currentStage.Focus();
            }
        }

        private Stage _currentStage;

        public StageController()
        {

        }

        public void AddStage(Stage stage)
        {
            Stages.Add(stage);
        }

        public void AddOnRevealListeners(Action<Stage> listener)
        {
            foreach (var stage in Stages)
            {
                stage.AddOnRevealListener(listener);
            }
        }

        public void AddOnFocusListeners(Action<Stage> listener)
        {
            foreach (var stage in Stages)
            {
                stage.AddOnFocusListener(listener);
            }
        }

        public void PassStage()
        {
            CurrentStage.Pass();
        }

        public void GoToStage(Stage stage)
        {
            stage.Focus();
        }

        public void UseLinearProgressionStructure()
        {
            ClearDependencyRelationships();
            var stages = new List<Stage>(Stages);

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
        }

        public void Commit()
        {
            InitializeStartingStages();
            AddOnFocusListeners(stage => _currentStage = stage);
        }

        private void InitializeStartingStages()
        {
            var stagesWithNoDependencies = Stages.Where(stage => stage.Dependencies.Count == 0);
            CurrentStage = stagesWithNoDependencies.First();
            foreach (var stage in stagesWithNoDependencies)
            {
                stage.Reveal();
            }
        }

        private void ClearDependencyRelationships()
        {
            foreach (var stage in Stages)
            {
                stage.ClearDependencyRelationships();
            }
        }
    }
}
