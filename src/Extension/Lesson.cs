
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
        private Func<string, Task<Challenge>> _challengeLookup;

        public Lesson(string name = null, IReadOnlyList<SubmitCode> setup = null)
        {
            Name = name;
            Setup = setup;
        }

        public Task StartChallengeAsync(Challenge challenge)
        {
            CurrentChallenge = challenge;
            if (CurrentChallenge is not null)
            {
                CurrentChallenge.Revealed = true;
                CurrentChallenge.Lesson = this;
            }
            return Task.CompletedTask;
        }

        public async Task StartChallengeAsync(string name)
        {
            var challenge = await _challengeLookup(name);
            if (challenge is not null)
            {
                await StartChallengeAsync(challenge);
            }
        }

        public void SetChallengeLookup(Func<string, Challenge> handler)
        {
            _challengeLookup = name =>
            {
                return Task.FromResult(handler(name));
            };
        }

        public void SetChallengeLookup(Func<string, Task<Challenge>> handler)
        {
            _challengeLookup = handler;
        }
    }
}
