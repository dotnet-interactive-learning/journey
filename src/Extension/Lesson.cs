
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
        public bool IsStartingChallenge { get; set; } = true;
        
        private List<Challenge> _challenges = new();

        public Lesson(string name = null)
        {
            Name = name;
        }

        public void AddChallenge(Challenge challenge)
        {
            _challenges.Add(challenge);
        }

        public async Task StartChallengeAsync(Challenge challenge)
        {
            if (challenge == null)
            {
                return;
            }
            CurrentChallenge = challenge;
            CurrentChallenge.Revealed = true;
            CurrentChallenge.Lesson = this;
            IsStartingChallenge = true;
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
            if (index == -1 || index + 1 >= _challenges.Count)
            {
                CurrentChallenge = null;
                return;
            }
            await StartChallengeAsync(_challenges[index + 1]);
        }
    }
}
