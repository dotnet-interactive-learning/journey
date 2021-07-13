using Extension.ChallengeControl;
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

        private Dictionary<string, Challenge> _challenges = new Dictionary<string, Challenge>();

        public Lesson()
        {

        }

        public void AddChallenge(string challengeId, Challenge challenge)
        {
            if (_challenges.ContainsKey(challengeId))
            {
                throw new ArgumentException($"Challenge already exists with id {challengeId}");
            }
            _challenges.Add(challengeId, challenge);
            ChallengeController.AddChallenge(challenge);
            challenge.Lesson = this;
        }

        public void GoToChallenge(string challengeId)
        {
            if (!_challenges.ContainsKey(challengeId))
            {
                throw new ArgumentException($"No challenge exists with id {challengeId}");
            }
            CurrentChallenge = _challenges[challengeId];
        }
    }
}
