
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

namespace Extension.Tests
{
    public class LessonTests : ProgressiveLearningTestBase
    {
        private Challenge GetChallenge(string name = null)
        {
            return new Challenge(name: name);
        }

        private List<SendEditableCode> GetSendEditableCode(string code)
        {
            return new List<SendEditableCode>
            {
                new SendEditableCode("csharp", code)
            };
        }

        [Fact]
        public async Task starting_to_an_unrevealed_challenge_directly_reveals_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();

            await lesson.StartChallengeAsync(challenge);

            challenge.Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task starting_a_challenge_sets_the_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();

            await lesson.StartChallengeAsync(challenge);

            lesson.CurrentChallenge.Should().Be(challenge);
        }

        [Fact]
        public async Task teacher_can_start_a_challenge_using_challenge_name()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenges = new Challenge[]
            {
                new Challenge(contents: GetSendEditableCode("1"), name: "1"),
                new Challenge(contents: GetSendEditableCode("2"), name: "2"),
                new Challenge(contents: GetSendEditableCode("3"), name: "3")
            }.ToList();
            challenges[0].OnCodeSubmittedAsync(async context =>
            {
                await context.StartChallengeAsync("3");
            });
            challenges.SetDefaultProgressionHandlers();
            await lesson.StartChallengeAsync(challenges[0]);
            lesson.SetChallengeLookup(name => challenges.FirstOrDefault(c => c.Name == name));

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenges[2]);
        }

        [Fact]
        public async Task teacher_can_explicitly_start_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenges = new Challenge[]
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
            await lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenges[1]);
        }

        [Fact]
        public async Task explicitly_starting_the_next_challenge_at_last_challenge_does_nothing()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenge = GetChallenge("1");
            challenge.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            await lesson.StartChallengeAsync(challenge);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(null);
        }

        [Fact]
        public async Task when_a_student_submits_code_to_a_challenge_they_move_to_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenges = new Challenge[]
            {
                GetChallenge("1"),
                GetChallenge("2"),
                GetChallenge("3")
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenges[1]);
        }

        [Fact]
        public async Task when_a_student_completes_the_last_challenge_then_the_lesson_is_completed()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var challenges = new Challenge[]
            {
                GetChallenge("1"),
                GetChallenge("2")
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await lesson.StartChallengeAsync(challenges[0]);

            await kernel.SubmitCodeAsync("1+1");
            await kernel.SubmitCodeAsync("2+1");

            lesson.CurrentChallenge.Should().Be(null);
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_starting_a_lesson()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            using var events = kernel.KernelEvents.ToSubscribedList();
            var setup = new SubmitCode[] {
                new SubmitCode("var a = 2;"),
                new SubmitCode("var b = 3;"),
                new SubmitCode("a = 4;")
            };
            var challenge = new Challenge(environmentSetup: setup);
            await lesson.StartChallengeAsync(challenge);
            await kernel.InitializeChallenge(challenge);

            await kernel.SubmitCodeAsync("a+b");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(7);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_starting_a_lesson()
        {
            var capturedCommands = new List<SendEditableCode>();
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            var contents = new SendEditableCode[] {
                new SendEditableCode("csharp", "var a = 2;"),
                new SendEditableCode("csharp", "var b = 3;"),
                new SendEditableCode("csharp", "a = 4;")
            };
            Challenge challenge = new Challenge(contents: contents);
            await lesson.StartChallengeAsync(challenge);
            await kernel.InitializeChallenge(challenge);

            await kernel.SubmitCodeAsync("a+b");

            capturedCommands.Should().BeEquivalentTo(contents);
        }

        [Fact]
        public async Task teacher_can_run_challenge_environment_setup_code_when_progressing_the_student_to_a_new_challenge()
        {
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            using var events = kernel.KernelEvents.ToSubscribedList();
            var setup = new SubmitCode[] {
                new SubmitCode("var a = 2;"),
                new SubmitCode("var b = 3;"),
                new SubmitCode("a = 4;")
            };
            var challenges = new Challenge[]
            {
                new Challenge(),
                new Challenge(environmentSetup: setup)
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await lesson.StartChallengeAsync(challenges[0]);
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("a + b");

            events.Should().ContainSingle<ReturnValueProduced>().Which.Value.Should().Be(7);
        }

        [Fact]
        public async Task teacher_can_show_challenge_contents_when_progressing_the_student_to_a_new_challenge()
        {
            var capturedCommands = new List<SendEditableCode>();
            var lesson = new Lesson();
            using var kernel = CreateKernel(lesson);
            var vscodeKernel = kernel.FindKernel("vscode");
            vscodeKernel.RegisterCommandHandler<SendEditableCode>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.CompletedTask;
            });
            using var events = kernel.KernelEvents.ToSubscribedList();
            var contents = new SendEditableCode[] {
                new SendEditableCode("csharp", "var a = 2;"),
                new SendEditableCode("csharp", "var b = 3;"),
                new SendEditableCode("csharp", "a = 4;")
            };
            var challenges = new Challenge[]
            {
                new Challenge(),
                new Challenge(contents: contents)
            }.ToList();
            challenges.SetDefaultProgressionHandlers();
            await lesson.StartChallengeAsync(challenges[0]);
            await kernel.SubmitCodeAsync("Console.WriteLine(1 + 1);");

            await kernel.SubmitCodeAsync("a + b");

            capturedCommands.Should().BeEquivalentTo(contents);
        }
    }
}
