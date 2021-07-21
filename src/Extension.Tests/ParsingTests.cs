using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class ParsingTests : ProgressiveLearningTestBase
    {
        private string GetStartupCommand(string relativeFilePath)
        {
            var prefix = Path.GetDirectoryName(Directory.GetCurrentDirectory());
            var path = Path.GetFullPath(Path.Combine(prefix, relativeFilePath));
            return $"#!start-lesson {path}";
        }

        [Fact]
        public async Task Test()
        {
            using var kernel = CreateKernel();

            await kernel.SubmitCodeAsync("#!start-lesson C:\\dev\\intern2021\\src\\Extension.Tests\\Notebooks\\notebook.dib");
        }
    }
}
