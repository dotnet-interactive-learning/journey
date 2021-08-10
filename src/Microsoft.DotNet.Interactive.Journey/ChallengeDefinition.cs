using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Interactive.Journey
{
    public class ChallengeDefinition
    {
        public string Name { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }
        public IReadOnlyList<SendEditableCode> Contents { get; }
        public IReadOnlyList<SubmitCode> EnvironmentSetup { get; }

        public ChallengeDefinition(string name, IReadOnlyList<SubmitCode> setup, IReadOnlyList<SendEditableCode> contents, IReadOnlyList<SubmitCode> environmentSetup)
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
}
