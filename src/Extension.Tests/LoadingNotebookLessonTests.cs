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
    public class LoadingNotebookLessonTests : ProgressiveLearningTestBase
    {
        [Fact]
        public async Task teacher_can_execute_lesson_setup_code()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");

            await kernel.SubmitCodeAsync("lessonSetupVar");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(666);
        }

        [Fact]
        public async Task teacher_can_use_add_rule_when_starting_a_lesson()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            await kernel.SubmitCodeAsync("1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "failrule",
                        "fail reasons",
                        "passrule",
                        "pass reasons"));
        }

        [Fact]
        public async Task teacher_can_use_on_code_submitted_when_starting_a_lesson()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            await kernel.SubmitCodeAsync("1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Good job"));
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_starting_a_lesson()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            await kernel.SubmitCodeAsync("challengeSetupVar");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(3);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_starting_a_lesson()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            capturedCommands.Select(c => c.Code).Join("\r\n")
                .Should().ContainAll(
                    "This is the LinkedList question.",
                    "// write your answer to LinkedList question below");
        }


        [Fact]
        public async Task when_starting_a_lesson_the_shown_challenge_contents_do_not_contain_directives()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            capturedCommands.Select(c => c.Code).Join("\r\n")
                .Should().NotContainAny(NotebookLessonParser.AllDirectiveNames);
        }

        [Fact]
        public async Task when_starting_a_lesson_the_shown_challenge_contents_do_not_contain_scratchpad_material()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            capturedCommands.Select(c => c.Code).Join("\r\n")
                .Should().NotContainAny("// random scratchpad stuff");
        }

        [Fact]
        public async Task teacher_can_use_add_rule_when_progressing_the_student_to_different_challenge()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("1 + 1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "dfsrule1",
                        "dfsfailreasons",
                        "dfsrule2",
                        "dfspassreasons"));
        }

        [Fact]
        public async Task teacher_can_use_on_code_submitted_when_progressing_the_student_to_different_challenge()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("1 + 1");

            events.Should().ContainSingle<DisplayedValueProduced>(
                e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                    .Value.ContainsAll(
                        "Good job for DFS"));
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_progressing_the_student_to_different_challenge()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("anotherChallengeSetupVar");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(10);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_progressing_the_student_to_different_challenge()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");

            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            var x = capturedCommands.Select(c => c.Code).Join("\r\n");
                x.Should().ContainAll(
                    "This is the DFS question.",
                    "// write your answer to DFS below");
        }

        [Fact]
        public async Task when_progressing_the_student_to_different_challenge_the_shown_challenge_contents_do_not_contain_directives()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\twoChallenges.dib")}");

            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            capturedCommands.Select(c => c.Code).Join("\r\n")
                .Should().NotContainAny(NotebookLessonParser.AllDirectiveNames);
        }

        [Fact]
        public async Task
            when_progressing_the_student_to_different_challenge_the_shown_challenge_contents_do_not_contain_scratchpad_material()
        {
            var capturedCommands = new List<SendEditableCode>();
            var kernel = CreateKernel();
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\singleChallenge.dib")}");

            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            capturedCommands.Select(c => c.Code).Join("\r\n")
                .Should().NotContainAny("// random scratchpad stuff");
        }

        [Fact]
        public async Task teacher_can_declare_identifiers_and_let_it_become_replaced_by_the_students_answer()
        {
            var kernel = CreateKernel();
            using var events = kernel.KernelEvents.ToSubscribedList();
            await kernel.SubmitCodeAsync($"#!start-lesson {GetNotebookPath(@"Notebooks\variableReplacing.dib")}");

            await kernel.SubmitCodeAsync(@"
CalcTrigArea = (double x, double y) => 0.5 * x * y;
");

            events
                .Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.FormattedValues.Single(v => v.MimeType == "text/html")
                        .Value.Contains("You passed"));
        }
    }
}
