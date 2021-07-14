
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class Lesson
    {
        public ChallengeController ChallengeController { get; private set; } = new ChallengeController();
        public Challenge CurrentChallenge
        {
            get
            {
                return ChallengeController.CurrentChallenge;
            }
            set
            {
                ChallengeController.CurrentChallenge = value;
            }
        }

        public Lesson()
        {

        }

        public void AddChallenge(Challenge challenge)
        {
            ChallengeController.AddChallenge(challenge);
            challenge.Lesson = this;
        }

// todo: remove pragma
#pragma warning disable 1998
        public async Task StartChallengeAsync(Challenge challenge)
#pragma warning restore 1998
        {
            CurrentChallenge = challenge;
            // todo: await someexternalendpoint.StartChallenge
        }
    }
}
