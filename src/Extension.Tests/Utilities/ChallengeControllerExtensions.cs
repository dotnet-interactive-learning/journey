using Extension.ChallengeControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class ChallengeControllerExtensions
    {
        public static List<Challenge> AddBlankChallenges(this ChallengeController challengeController, int numberOfChallenges)
        {
            var challenges = new List<Challenge>();

            for (int i = 0; i < numberOfChallenges; i++)
            {
                var challenge = new Challenge(Enumerable.Empty<string>());
                challenges.Add(challenge);
                challengeController.AddChallenge(challenge);
            }

            return challenges;
        }
    }
}
