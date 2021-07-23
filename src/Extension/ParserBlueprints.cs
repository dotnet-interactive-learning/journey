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

        public Challenge ToChallenge()
        {
            return new Challenge(Setup,Contents,EnvironmentSetup, Name);
        }
    }

    public class LessonBlueprint
    {
        public string Name { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }

        public LessonBlueprint(string name, IReadOnlyList<SubmitCode> setup)
        {
            Name = name;
            Setup = setup;
        }

        public Lesson ToLesson()
        {
            Lesson lesson = new(Name, Setup);
            return lesson;
        }
    }
}
