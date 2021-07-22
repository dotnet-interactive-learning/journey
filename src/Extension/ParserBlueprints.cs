using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class ChallengeBlueprint
    {
        public string Name { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }
        public IReadOnlyList<SendEditableCode> Contents { get; }
        public IReadOnlyList<SubmitCode> EnvironmentSetup { get; }

        public ChallengeBlueprint(string name, IReadOnlyList<SubmitCode> setup, IReadOnlyList<SendEditableCode> contents, IReadOnlyList<SubmitCode> environmentSetup)
        {
            Name = name;
            Setup = setup;
            Contents = contents;
            EnvironmentSetup = environmentSetup;
        }
    }

    public class LessonBlueprint
    {
        public string Name { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }
        public IReadOnlyList<ChallengeBlueprint> Challenges { get; }

        public LessonBlueprint(string name, IReadOnlyList<SubmitCode> setup, IReadOnlyList<ChallengeBlueprint> challenges)
        {
            Name = name;
            Setup = setup;
            Challenges = challenges;
        }

        public Lesson ToLesson()
        {
            List<Challenge> challenges = new();
            foreach (var c in Challenges)
            {
                challenges.Add(new Challenge(c.Setup, c.Contents, c.EnvironmentSetup, c.Name));
            }

            Lesson lesson = new(Name, Setup);
            foreach (var c in challenges)
            {
                lesson.AddChallenge(c);
            }
            return lesson;
        }
    }
}
