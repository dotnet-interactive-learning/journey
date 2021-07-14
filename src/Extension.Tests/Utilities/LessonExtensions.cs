
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class LessonExtensions
    {
        public static Dictionary<string, Challenge> AddBlankChallenges(this Lesson lesson, params string[] challengeIds)
        {
            var challenges = new Dictionary<string, Challenge>();
            foreach (var id in challengeIds)
            {
                var challenge = new Challenge(new EditableCode[] { });
                challenges.Add(id, challenge);
                lesson.AddChallenge(challenge);
            }
            return challenges;
        }
    }
}
