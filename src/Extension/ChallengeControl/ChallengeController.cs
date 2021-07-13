using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.ChallengeControl
{
    public class ChallengeController
    {
        public OrderedTable<Challenge> Challenges { get; set; } = new OrderedTable<Challenge>();
        public Challenge CurrentChallenge
        {
            get
            {
                return _currentChallenge;
            }
            set
            {
                _currentChallenge = value;
                _currentChallenge.Focus();
            }
        }

        private Challenge _currentChallenge;

        public ChallengeController()
        {

        }

        public void AddChallenge(Challenge challenge)
        {
            Challenges.Add(challenge);
        }

        public void AddOnRevealListeners(Action<Challenge> listener)
        {
            foreach (var challenge in Challenges)
            {
                challenge.AddOnRevealListener(listener);
            }
        }

        public void AddOnFocusListeners(Action<Challenge> listener)
        {
            foreach (var challenge in Challenges)
            {
                challenge.AddOnFocusListener(listener);
            }
        }

        public void UseLinearProgressionStructure()
        {
            ClearDependencyRelationships();
            var challenges = new List<Challenge>(Challenges);

            if (challenges.Count >= 2)
            {
                var prevChallenge = challenges[0];
                for (int i = 1; i < challenges.Count; i++)
                {
                    var currChallenge = challenges[i];
                    currChallenge.AddDependency(prevChallenge);
                    prevChallenge.AddDependent(currChallenge);
                    prevChallenge = currChallenge;
                }
            }
        }

        public void Commit()
        {
            InitializeStartingChallenges();
            AddOnFocusListeners(challenge => _currentChallenge = challenge);
        }

        private void InitializeStartingChallenges()
        {
            var challengesWithNoDependencies = Challenges.Where(challenge => challenge.Dependencies.Count == 0);
            CurrentChallenge = challengesWithNoDependencies.First();
            foreach (var challenge in challengesWithNoDependencies)
            {
                challenge.Reveal();
            }
        }

        private void ClearDependencyRelationships()
        {
            foreach (var challenge in Challenges)
            {
                challenge.ClearDependencyRelationships();
            }
        }
    }
}
