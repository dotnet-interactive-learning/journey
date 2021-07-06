using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Extension
{
    public static class KernelExtensions
    {
        public static T UseHiddenCellMagicCommand<T>(this T kernel, CellController cellController)
            where T : Kernel
        {
            var commandName = "#!hidden";
            var hiddenCellCommand = new Command(commandName, "This cell can be revealed")
            {
                new Argument<string>("hiddenId", "Hidden cell number")
            };

            hiddenCellCommand.Handler = CommandHandler.Create<string, KernelInvocationContext>((hiddenId, context) =>
            {
                if (context.Command is SubmitCode submitCode)
                {
                    var newCode = "";
                    var lines = Regex.Split(submitCode.Code, "\r\n|\n|\r");
                    var filteredLines = lines.Where(line => !line.TrimStart().StartsWith(commandName));
                    newCode = string.Join(Environment.NewLine, filteredLines);

                    cellController.AddHiddenCell(hiddenId, HiddenCell.FromCode(newCode));
                }
            });

            kernel.AddDirective(hiddenCellCommand);

            return kernel;
        }
    }
}