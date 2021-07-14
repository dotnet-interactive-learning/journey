
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class Lesson
    {
        public Challenge CurrentChallenge { get; private set; }

        public Lesson()
        {

        }

        // todo: remove pragma
#pragma warning disable 1998
        public async Task StartChallengeAsync(Challenge challenge)
#pragma warning restore 1998
        {
            CurrentChallenge = challenge;
            CurrentChallenge.Focus();
            // todo: await someexternalendpoint.StartChallenge that sends EditableCode
        }
    }
}
