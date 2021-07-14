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
        public OrderedTable<Challenge> Challenges { get; private set; } = new();
        public Dictionary<Challenge, List<Challenge>> Dependencies { get; private set; } = new();
        public Dictionary<Challenge, List<Challenge>> Dependents { get; private set; } = new();
        public Dictionary<Challenge, bool> HasPassed { get; private set; } = new();
        public Challenge CurrentChallenge => _lesson.CurrentChallenge;

        private Lesson _lesson = new();

        public ChallengeGraphProgressionService(Lesson lesson)
        {
            _lesson = lesson;
        }

        public ChallengeGraphProgressionService(Lesson lesson, IReadOnlyList<Challenge> challenges)
            : this(lesson)
        {
            foreach (var challenge in challenges)
            {
                Challenges.Add(challenge);
            }
        }

        public void AddChallenge(Challenge challenge, bool hasPassed = false)
        {
            Challenges.Add(challenge);
            HasPassed.Add(challenge, hasPassed);
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
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        public async Task ForceGoToChallengeAsync(Challenge challenge)
        {
            await _lesson.StartChallengeAsync(challenge);
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
                    AddDependency(currChallenge, prevChallenge);
                    prevChallenge = currChallenge;
                }
            }
        }

        public async Task Commit()
        {
            foreach (var challenge in Challenges)
            {
                if (!Dependencies.ContainsKey(challenge))
                {
                    Dependencies.Add(challenge, new List<Challenge>());
                }
                if (!Dependents.ContainsKey(challenge))
                {
                    Dependents.Add(challenge, new List<Challenge>());
                }
                if (!HasPassed.ContainsKey(challenge))
                {
                    HasPassed.Add(challenge, false);

                }
            }
            await InitializeStartingChallenges();
        }

        private async Task InitializeStartingChallenges()
        {
            var challengesWithNoDependencies = Challenges.
                Where(c => Dependencies[c].Count() == 0);
            if (challengesWithNoDependencies.Count() == 0)
            {
                throw new InvalidOperationException("There are no challenges with no dependencies");
            }
            await _lesson.StartChallengeAsync(challengesWithNoDependencies.First());
            foreach (var challenge in challengesWithNoDependencies)
            {
                challenge.Reveal();
            }
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
            Dependencies.Clear();
            Dependents.Clear();
        }
    }
}
