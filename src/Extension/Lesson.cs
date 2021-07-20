
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

        private List<Challenge> _challenges = new();

        public Lesson(string name = null)
        {
            Name = name;
        }

        public void AddChallenge(Challenge challenge)
        {
            _challenges.Add(challenge);
        }

        // todo: remove pragma
#pragma warning disable 1998
        public async Task StartChallengeAsync(Challenge challenge)
#pragma warning restore 1998
        {
            if (challenge == null)
            {
                return;
            }
            CurrentChallenge = challenge;
            CurrentChallenge.Revealed = true;
            CurrentChallenge.Lesson = this;
            // todo: await someexternalendpoint.StartChallenge that sends EditableCode
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
                return;
            }
            await StartChallengeAsync(_challenges[index + 1]);
        }

        public bool IsSetupCommand(KernelCommand command)
        {
            return CurrentChallenge.Setup.Any(s => s == command);
        }
    }
}
