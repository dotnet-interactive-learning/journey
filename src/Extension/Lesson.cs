
using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class Lesson
    {
        public string Name { get; }
        public Challenge CurrentChallenge { get; private set; }
        public IReadOnlyList<SubmitCode> Setup { get; private set; }
        
        private List<Challenge> _challenges = new();

        public Lesson(string name = null, IReadOnlyList<SubmitCode> setup = null)
        {
            Name = name;
            Setup = setup;
        }

        internal void AddChallenge(Challenge challenge)
        {
            if (string.IsNullOrWhiteSpace(challenge.Name))
            {
                challenge.Name = $"Challenge {_challenges.Count + 1}";
            }
            _challenges.Add(challenge);
        }

        public Task StartChallengeAsync(Challenge challenge)
        {
            if (challenge == null)
            {
                return Task.CompletedTask;
            }

            if (!_challenges.Contains(challenge))
            {
                _challenges.Add(challenge);
            }
            CurrentChallenge = challenge;
            CurrentChallenge.Revealed = true;
            CurrentChallenge.Lesson = this;
            return Task.CompletedTask;
        }

        public Task StartLessonAsync()
        {
            var challenge = _challenges.FirstOrDefault();
            return StartChallengeAsync(challenge);
        }

        public async Task StartChallengeAsync(string name)
        {
            var challenge = _challenges.FirstOrDefault(c => c.Name == name);
            if (challenge is not null)
            {
                await StartChallengeAsync(challenge);
            }
        }

        public async Task StartNextChallengeAsync()
        {
            var index = _challenges.FindIndex(c => c == CurrentChallenge);
            index++;
            if (index < 0 || index >= _challenges.Count)
            {
                CurrentChallenge = null;
                return;
            }
            await StartChallengeAsync(_challenges[index]);
        }
    }
}
