using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public static class ChallengeExtensions
    {
        public static Challenge SetDefaultProgression(this Challenge challenge, Challenge nextChallenge)
        {
            challenge.DefaultOnCodeSubmittedHandler = async context => await context.StartChallengeAsync(nextChallenge);
            return nextChallenge;
        }

        public static void SetDefaultProgression(this IEnumerable<Challenge> challenges)
        {
            var cs = challenges.ToList();
            for (int i = 0; i < cs.Count - 1; i++)
            {
                cs[i].SetDefaultProgression(cs[i + 1]);
            }
        }
    }
}
