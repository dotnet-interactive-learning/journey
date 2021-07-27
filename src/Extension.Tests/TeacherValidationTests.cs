using System;
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
namespace Extension.Tests
{
    public class TeacherValidationTests : ProgressiveLearningTestBase
    {
        private async Task RunAllCells(string fileName, byte[] document, CompositeKernel kernel)
        {
            var notebook = kernel.ParseNotebook(fileName, document);
            foreach (var cell in notebook.Cells)
            {
                await kernel.SendAsync(new SubmitCode(cell.Contents, cell.Language));
            }
        }

        [Fact]
        public async Task teacher_can_use_scratchpad_to_validate_their_material()
        {
            var filename = "teacherValidation.dib";
            var file = new FileInfo(GetNotebookPath($@"Notebooks\{filename}"));
            var document = await File.ReadAllBytesAsync(file.FullName);
            var lesson = new Lesson();
            var kernel = await CreateKernel(lesson, true);
            using var events = kernel.KernelEvents.ToSubscribedList();

            await RunAllCells(filename, document, kernel);

            events
                .OfType<DisplayedValueProduced>()
                .Should()
                .SatisfyRespectively(new Action<DisplayedValueProduced>[]
                {
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.ContainsAll(
                            "Challenge func rule failed",
                            "Challenge func not done"),
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.ContainsAll(
                            "Challenge func rule passed",
                            "Challenge func completed"),
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.ContainsAll(
                            "Challenge math rule passed",
                            "Challenge math message"),
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.ContainsAll(
                            "Challenge math rule failed",
                            "Challenge math message")
                });
        }
    }
}
