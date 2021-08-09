using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive.Journey
{
    public static class ChallengeExtensions
    {
        public static Challenge SetDefaultProgressionHandler(this Challenge challenge, Challenge nextChallenge)
        {
            challenge.DefaultProgressionHandler = async context => await context.StartChallengeAsync(nextChallenge);
            return nextChallenge;
        }

        public static void SetDefaultProgressionHandlers(this List<Challenge> challenges)
        {
            for (int i = 0; i < challenges.Count - 1; i++)
            {
                challenges[i].SetDefaultProgressionHandler(challenges[i + 1]);
            }
            challenges.Last().DefaultProgressionHandler = async context =>
            {
                await context.StartChallengeAsync(null as Challenge);
            };
        }
    }
}
