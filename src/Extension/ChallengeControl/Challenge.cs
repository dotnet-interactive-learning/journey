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
        public Lesson Lesson { get; set; }
        public IEnumerable<string> Contents { get; private set; }
        public bool Passed { get; private set; } = false;
        public bool Revealed { get; private set; } = false;
        public List<Challenge> Dependencies { get; private set; } = new List<Challenge>();
        public List<Challenge> Dependents { get; private set; } = new List<Challenge>();
        public Action<Challenge, Lesson> OnEvaluationCompleteHandler { get; private set; }

        private List<Action<Challenge>> _onRevealListeners = new List<Action<Challenge>>();
        private List<Action<Challenge>> _onFocusListeners = new List<Action<Challenge>>();

        public Challenge(IEnumerable<string> content, Lesson lesson = null)
        {
            Contents = content;
            Lesson = lesson;
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
            _onRevealListeners.Add(listener);
        }

        public void AddOnFocusListener(Action<Challenge> listener)
        {
            _onFocusListeners.Add(listener);
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
            foreach (var listener in _onFocusListeners)
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
                foreach (var listener in _onRevealListeners)
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

        public void OnEvaluationComplete(Action<Challenge, Lesson> handler)
        {
            OnEvaluationCompleteHandler = handler;
        }

        public void InvokeOnEvaluationComplete()
        {
            OnEvaluationCompleteHandler(this, Lesson);
        }
    }
}
