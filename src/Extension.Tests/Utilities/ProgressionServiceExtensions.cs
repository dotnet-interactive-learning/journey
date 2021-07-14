
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class ProgressionServiceExtensions
    {
        public static List<Challenge> AddBlankChallenges(this ChallengeGraphProgressionService service, int numberOfChallenges)
        {
            var challenges = new List<Challenge>();

            for (int i = 0; i < numberOfChallenges; i++)
            {
                var challenge = new Challenge(new EditableCode[] { });
                challenges.Add(challenge);
                service.AddChallenge(challenge);
            }

            return challenges;
        }
    }
}
