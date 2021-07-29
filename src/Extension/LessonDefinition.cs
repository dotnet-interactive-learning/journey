using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class LessonDefinition
    {
        public string Name { get; }
        public IReadOnlyList<SubmitCode> Setup { get; }

        public LessonDefinition(string name, IReadOnlyList<SubmitCode> setup)
        {
            Name = name;
            Setup = setup;
        }
    }
}
