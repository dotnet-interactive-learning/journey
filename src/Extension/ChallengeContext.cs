using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeContext
    {
        public Lesson Lesson { get; private set; }

        public List<Evaluation> RuleEvaluations { get; private set; }

        public ChallengeContext(Lesson lesson)
        {
            Lesson = lesson;
        }

        public void Pass(string message)
        {
            throw new NotImplementedException();
        }

        public void Fail(string message)
        {
            throw new NotImplementedException();
        }

        public async Task StartChallengeAsync(Challenge challenge)
        {
            await Lesson.StartChallengeAsync(challenge);
        }
    }
}
