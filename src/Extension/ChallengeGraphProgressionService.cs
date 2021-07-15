using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeGraphProgressionService
    {
        public Dictionary<Challenge, List<Challenge>> Dependencies { get; private set; } = new();
        public Dictionary<Challenge, List<Challenge>> Dependents { get; private set; } = new();
        public Dictionary<Challenge, bool> HasPassed { get; private set; } = new();
        public Challenge CurrentChallenge => _lesson.CurrentChallenge;

        private OrderedTable<Challenge> _challenges = new();

        private Lesson _lesson = new();

        public ChallengeGraphProgressionService(Lesson lesson)
        {
            _lesson = lesson;
        }

        public void AddChallenge(Challenge challenge, bool hasPassed = false)
        {
            _challenges.Add(challenge);
            HasPassed.Add(challenge, hasPassed);
            if (!Dependencies.ContainsKey(challenge))
            {
                Dependencies.Add(challenge, new List<Challenge>());
            }
        }

        public void Pass()
        {
            Pass(CurrentChallenge);
        }

        public void Pass(Challenge challenge)
        {
            HasPassed[challenge] = true;
        }

        public async Task<bool> TryGoToNextChallengeAsync()
        {
            if (!_challenges.Contains(CurrentChallenge))
            {
                return false;
            }
            var firstDependentWithResolvedDependencies = Dependents[CurrentChallenge]
                .FirstOrDefault(dependent => Dependencies[dependent].All(dependency => HasPassed[dependency]));
            if (firstDependentWithResolvedDependencies is null)
            {
                return false;
            }
            else
            {
                await _lesson.StartChallengeAsync(firstDependentWithResolvedDependencies);
                return true;
            }
        }

        public async Task<bool> TryGoToFurthestChallengeAsync()
        {
            // try to go to the "most advanced" challenges in the graph
            // "most advanced" challenges
            // means the set of challenges who is the furthest from the current challenge
            // in the transpose of the dependency graph (which is dependent graph)
            // who has all dependency challenges passed
            // use BFS in the dependent graph
            await Task.Yield();
            throw new NotImplementedException();
        }

        public async Task GoToChallengeAsync(Challenge challenge)
        {
            await _lesson.StartChallengeAsync(challenge);
        }

        public void UseLinearProgressionStructure()
        {
            ClearDependencyRelationships();
            var challenges = new List<Challenge>(_challenges);

            if (challenges.Count >= 2)
            {
                var prevChallenge = challenges[0];
                for (int i = 1; i < challenges.Count; i++)
                {
                    var currChallenge = challenges[i];
                    AddDependency(currChallenge, prevChallenge);
                    prevChallenge = currChallenge;
                }
            }
        }

        public async Task StartProgression()
        {
            await InitializeStartingChallenges();
        }

        private async Task InitializeStartingChallenges()
        {
            var challengesWithNoDependencies = _challenges.
                Where(c => Dependencies[c].Count() == 0);
            if (challengesWithNoDependencies.Count() == 0)
            {
                throw new InvalidOperationException("There are no challenges with no dependencies");
            }
            await _lesson.StartChallengeAsync(challengesWithNoDependencies.First());
        }

        public void AddDependency(Challenge subject, Challenge dependency)
        {
            if (!Dependencies.ContainsKey(subject))
            {
                Dependencies.Add(subject, new List<Challenge>());
            }
            Dependencies[subject].Add(dependency);
            if (!Dependents.ContainsKey(dependency))
            {
                Dependents.Add(dependency, new List<Challenge>());
            }
            Dependents[dependency].Add(subject);
        }

        private void ClearDependencyRelationships()
        {
            foreach (var dependencyList in Dependencies.Values)
            {
                dependencyList.Clear();
            }
            foreach (var dependentList in Dependents.Values)
            {
                dependentList.Clear();
            }
        }
    }
}
