
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

        public void GoToChallenge(Challenge challenge)
        {
            CurrentChallenge = challenge;
        }
    }
}
