using System;
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using System.IO;
using System.Linq;
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
            var kernel = await CreateBootstrappedKernel(lesson, true);
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

        [Fact]
        public async Task teacher_can_use_add_rule_when_starting_a_lesson()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\teacherValidation.dib")}");

            await kernel.SubmitCodeAsync("1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Challenge func rule",
                        "Challenge func rule failed"));
        }

        [Fact]
        public async Task teacher_can_use_on_code_submitted_when_starting_a_lesson()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\teacherValidation.dib")}");

            await kernel.SubmitCodeAsync("1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Challenge func not done"));
        }

        [Fact]
        public async Task teacher_can_use_add_rule_when_progressing_the_student_to_different_challenge()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\teacherValidation.dib")}");
            await kernel.SubmitCodeAsync("CalculateTriangleArea = (double x, double y) => 0.5 * x * y;");

            await kernel.SubmitCodeAsync("Math.Sqrt(pi)");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Challenge math rule",
                        "Challenge math passed"));
        }

        [Fact]
        public async Task teacher_can_use_on_code_submitted_when_progressing_the_student_to_different_challenge()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\teacherValidation.dib")}");
            await kernel.SubmitCodeAsync("CalculateTriangleArea = (double x, double y) => 0.5 * x * y;");

            await kernel.SubmitCodeAsync("Math.Sqrt(pi)");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Challenge math message"));
        }
    }
}
