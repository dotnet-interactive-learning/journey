using Extension.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.ChallengeControl
{
    public class Challenge
    {
        public IEnumerable<string> Contents { get; private set; }
        public bool Passed { get; set; } = false;
        public bool Revealed { get; set; } = false;

        public List<Challenge> Dependencies { get; set; } = new List<Challenge>();
        public List<Challenge> Dependents { get; set; } = new List<Challenge>();

        public List<Action<Challenge>> OnRevealListeners { get; set; } = new List<Action<Challenge>>();
        public List<Action<Challenge>> OnFocusListeners { get; set; } = new List<Action<Challenge>>();

        public Challenge(IEnumerable<string> content)
        {
            Contents = content;
        }

        public void AddDependency(Challenge challenge)
        {
            Dependencies.Add(challenge);
        }

        public void AddDependent(Challenge challenge)
        {
            Dependents.Add(challenge);
        }

        public void AddOnRevealListener(Action<Challenge> listener)
        {
            OnRevealListeners.Add(listener);
        }

        public void AddOnFocusListener(Action<Challenge> listener)
        {
            OnFocusListeners.Add(listener);
        }

        public void Pass()
        {
            Passed = true;
            foreach (var dependent in Dependents)
            {
                if (dependent.Revealed || dependent.CanReveal())
                {
                    dependent.Focus();
                }
            }
        }

        public void Focus()
        {
            foreach (var listener in OnFocusListeners)
            {
                listener(this);
            }
            Reveal();
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

        public void ClearDependencyRelationships()
        {
            Dependencies.Clear();
            Dependents.Clear();
        }
    }
}
