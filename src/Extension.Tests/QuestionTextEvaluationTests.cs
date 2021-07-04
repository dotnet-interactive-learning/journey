using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Extension.Tests.Utilities;
using Extension.Events;
using System;

namespace Extension.Tests
{
    public class QuestionTextEvaluationTests
    {

        [Fact]
        public async Task submitting_text_that_fails_evaluation_criterion_gives_failed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            evaluator.AddQuestionTextCriterion("1", s => s.Contains("3.14"));

            await kernel.SubmitCodeAsync(@"
#!markdown
#!question 1
3.13
");
            events.Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }

        [Fact]
        public async Task submitting_text_that_passes_evaluation_criterion_gives_passed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            evaluator.AddQuestionTextCriterion("1", s => s.Contains("3.14"));

            await kernel.SubmitCodeAsync(@"
#!markdown
#!question 1
3.14
");
            events.Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }


        [Fact]
        public async Task submitting_text_without_setting_evaluation_criterion_gives_passed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(@"
#!markdown
#!question 1
3.14
");

            events.Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }


        [Fact]
        public async Task multiple_evaluation_criteria_is_matched_to_the_correct_question()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            evaluator.AddQuestionTextCriterion("1", s => s.Contains("3.14"));
            evaluator.AddQuestionTextCriterion("2", s => s.Contains("2.72"));

            var resultQuestion2 = await kernel.SubmitCodeAsync(@"
#!markdown
#!question 2
3.14
");

            var resultQuestion1 = await kernel.SubmitCodeAsync(@"
#!markdown
#!question 1
2.72
");

            resultQuestion1.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);

            resultQuestion2.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }


        [Fact]
        public async Task multiple_evaluation_criteria_for_one_question_is_evaluated_as_a_conjunction()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            evaluator.AddQuestionTextCriterion("1", s => s.Contains("3.14"));
            evaluator.AddQuestionTextCriterion("1", s => s.Contains("2.72"));

            await kernel.SubmitCodeAsync(@"
#!markdown
#!question 1
3.13 2.72
");

            events.Should().ContainSingle<QuestionTextEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }
    }
}
