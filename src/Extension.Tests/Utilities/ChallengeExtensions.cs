using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class ChallengeExtensions
    {
        public static IEnumerable<bool> GetRevealedStatuses(this IEnumerable<Challenge> challenges)
        {
            return challenges.Select(c => c.Revealed);
        }
    }
}
