using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Xunit;

namespace Extension.Tests
{
    public class RevealCellTests
    {
        [Fact]
        public async Task revealing_a_cell_produces_add_cell_command()
        {
            var controller = new CellController();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel.UseHiddenCellMagicCommand(controller);

            await kernel.SubmitCodeAsync(@"
#!hidden 1

var revealedVar = 10;
");

            var commands = controller.GetAddCellCommands("1");

            // will fix to use ContainSingle<KernelCommand>()
            commands.Should().HaveCount(1);
            commands.First().Should().Match(cmd => ((AddCell)cmd).Contents.Contains("var revealedVar = 10;"));
        }
    }
}
