
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Extension.Tests
{
    public class LessonTests : ProgressiveLearningTestBase
    {
        private Challenge GetChallenge(string name = null)
        {
            return new(name: name);
        }

        private List<SendEditableCode> GetSendEditableCode(string code)
        {
            return new()
            {
                new SendEditableCode("csharp", code)
            };
        }

        [Fact]
        public async Task starting_to_an_unrevealed_challenge_directly_reveals_it()
        {
            var challenge = GetChallenge();

            await Lesson.StartChallengeAsync(challenge);

            challenge.Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task starting_a_challenge_sets_the_current_challenge_to_it()
        {
            var challenge = GetChallenge();

            await Lesson.StartChallengeAsync(challenge);

            Lesson.CurrentChallenge.Should().Be(challenge);
        }

        [Fact]
        public async Task teacher_can_start_a_challenge_using_challenge_name()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenges = new Challenge[]
            {
                new(contents: GetSendEditableCode("1"), name: "1"),
                new(contents: GetSendEditableCode("2"), name: "2"),
                new(contents: GetSendEditableCode("3"), name: "3")
            }.ToList();
            challenges[0].OnCodeSubmittedAsync(async context =>
            {
                await context.StartChallengeAsync("3");
            });
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);
            Lesson.SetChallengeLookup(name => challenges.FirstOrDefault(c => c.Name == name));

            await kernel.SubmitCodeAsync("1+1");

            Lesson.CurrentChallenge.Should().Be(challenges[2]);
        }

        [Fact]
        public async Task teacher_can_explicitly_start_the_next_challenge()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenges = new[]
            {
                GetChallenge("1"),
                GetChallenge("2"),
                GetChallenge("3")
            }.ToList();
            challenges[0].OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");

            Lesson.CurrentChallenge.Should().Be(challenges[1]);
        }

        [Fact]
        public async Task teacher_can_stay_at_the_current_challenge()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenges = new[]
            {
                GetChallenge("1"),
                GetChallenge("2"),
                GetChallenge("3")
            }.ToList();
            challenges[0].OnCodeSubmitted(context =>
            {
                context.SetMessage("hi");
            });
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");

            Lesson.CurrentChallenge.Should().Be(challenges[0]);
        }

        [Fact]
        public async Task when_teacher_chooses_to_stay_at_the_current_challenge_the_next_challenge_is_not_revealed()
        {
            var capturedCommands = new List<SendEditableCode>();
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            var contents = new SendEditableCode[] {
                new("csharp", "var a = 2;"),
                new("csharp", "var b = 3;"),
                new("csharp", "a = 4;")
            };
            var challenges = new Challenge[]
            {
                new(),
                new(contents: contents)
            }.ToList();
            challenges[0].OnCodeSubmitted(context => { });
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            capturedCommands.Should().BeEmpty();
        }

        [Fact]
        public async Task explicitly_starting_the_next_challenge_at_last_challenge_does_nothing()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenge = GetChallenge("1");
            challenge.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            await Lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("1+1");

            Lesson.CurrentChallenge.Should().Be(null);
        }

        [Fact]
        public async Task when_a_student_submits_code_to_a_challenge_they_move_to_the_next_challenge()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenges = new[]
            {
                GetChallenge("1"),
                GetChallenge("2"),
                GetChallenge("3")
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");

            Lesson.CurrentChallenge.Should().Be(challenges[1]);
        }

        [Fact]
        public async Task when_a_student_completes_the_last_challenge_then_the_Lesson_is_completed()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var challenges = new[]
            {
                GetChallenge("1"),
                GetChallenge("2")
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");
            await kernel.SubmitCodeAsync("2+1");

            Lesson.CurrentChallenge.Should().Be(null);
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_starting_a_Lesson()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            using var events = kernel.KernelEvents.ToSubscribedList();
            var setup = new SubmitCode[] {
                new("var a = 2;"),
                new("var b = 3;"),
                new("a = 4;")
            };
            var challenge = new Challenge(environmentSetup: setup);
            await Lesson.StartChallengeAsync(challenge);
            await kernel.InitializeChallenge(challenge);

            await kernel.SubmitCodeAsync("a+b");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(7);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_starting_a_Lesson()
        {
            var capturedCommands = new List<SendEditableCode>();
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            var contents = new SendEditableCode[] {
                new("csharp", "var a = 2;"),
                new("csharp", "var b = 3;"),
                new("csharp", "a = 4;")
            };
            Challenge challenge = new Challenge(contents: contents);
            await Lesson.StartChallengeAsync(challenge);
            await kernel.InitializeChallenge(challenge);

            await kernel.SubmitCodeAsync("a+b");

            capturedCommands.Should().BeEquivalentTo(contents);
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_progressing_the_student_to_a_new_challenge()
        {
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            using var events = kernel.KernelEvents.ToSubscribedList();
            var setup = new SubmitCode[] {
                new("var a = 2;"),
                new("var b = 3;"),
                new("a = 4;")
            };
            var challenges = new Challenge[]
            {
                new(),
                new(environmentSetup: setup)
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("a + b");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(7);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_progressing_the_student_to_a_new_challenge()
        {
            var capturedCommands = new List<SendEditableCode>();
            using var kernel = await CreateKernel(LessonMode.StudentMode);
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            var contents = new SendEditableCode[] {
                new("csharp", "var a = 2;"),
                new("csharp", "var b = 3;"),
                new("csharp", "a = 4;")
            };
            var challenges = new Challenge[]
            {
                new(),
                new(contents: contents)
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await Lesson.StartChallengeAsync(challenges[0]);
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("a + b");

            capturedCommands.Should().BeEquivalentTo(contents);
        }
    }
}
