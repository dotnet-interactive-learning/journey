using Extension.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.StageControl
{
    public class Stage
    {
        public int StageId { get; }
        public IEnumerable<string> Contents { get; private set; }
        public bool Passed { get; private set; } = false;
        public bool Revealed { get; private set; } = false;

        public List<Stage> Dependencies { get; private set; } = new List<Stage>();
        public List<Stage> Dependents { get; private set; } = new List<Stage>();

        public List<Action<Stage>> OnRevealListeners { get; private set; } = new List<Action<Stage>>();

        public Stage(int stageId, IEnumerable<string> content)
        {
            StageId = stageId;
            Contents = content;
        }

        public void AddDependency(Stage stage)
        {
            Dependencies.Add(stage);
        }

        public void AddDependent(Stage stage)
        {
            Dependents.Add(stage);
        }

        public void AddOnRevealListener(Action<Stage> listener)
        {
            OnRevealListeners.Add(listener);
        }

        public void Pass()
        {
            Passed = true;
            foreach (var dependent in Dependents)
            {
                dependent.TryReveal();
            }
        }

        public bool CanReveal()
        {
            return Dependencies
                .Select(dependency => dependency.Passed)
                .All(passed => passed);
        }

        public void Reveal()
        {
            if (!Revealed)
            {
                foreach (var listener in OnRevealListeners)
                {
                    listener(this);
                }
                Revealed = true; 
            }
        }

        public void TryReveal()
        {
            if (CanReveal())
            {
                Reveal();
            }
        }

        public void ClearDependencyRelationships()
        {
            Dependencies.Clear();
            Dependents.Clear();
        }
    }
}
