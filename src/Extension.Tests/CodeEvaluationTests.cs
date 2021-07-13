using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Extension.Tests.Utilities;
using Extension.Events;

namespace Extension.Tests
{
    public class CodeEvaluationTests
    {
        private string EvaluateRectangeArea(string questionId = "1")
        {
            return $@"
#!evaluate {questionId}
area(10, 10) == 100
";
        }
        
        private string EvaluateTriangleArea(string questionId = "1")
        {
            return $@"
#!evaluate {questionId}
area(10, 10) == 50
";
        }

        private string EvaluateAlwaysPassing(string questionId = "1")
        {
            return $@"
#!evaluate {questionId}
true
";
        }

        private string StudentAnswerRectangleArea(string questionId = "1")
        {
            return $@"
#!question {questionId}
double area(double x, double y) 
{{
    return x * y;
}}
";
        }

        private string StudentAnswerTriangleArea(string questionId = "1")
        {
            return $@"
#!question {questionId}
double area(double x, double y) 
{{
    return 0.5 * x * y;
}}
";
        }

        [Fact]
        public async Task submitting_code_that_fails_evaluation_criterion_gives_failed_evaluation()
        {
            var evaluator = new Evaluator();       
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            // instructor provides testcase script which returns bool
            await kernel.SubmitCodeAsync(EvaluateTriangleArea());

            // students code runs
            await kernel.SubmitCodeAsync(StudentAnswerRectangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }

        [Fact]
        public async Task submitting_code_that_passes_evaluation_criterion_gives_passed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            // instructor provides testcase script which returns bool
            await kernel.SubmitCodeAsync(EvaluateTriangleArea());

            // students code runs
            await kernel.SubmitCodeAsync(StudentAnswerTriangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }


        [Fact]
        public async Task forward_symbol_declaration_will_not_affect_evaluation_criteria_results()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(@"
double area(double x, double y) => 0.0
");

            await kernel.SubmitCodeAsync(EvaluateTriangleArea());

            // students code runs
            await kernel.SubmitCodeAsync(StudentAnswerTriangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }

        // discussion required
        [Fact]
        public async Task submitting_code_without_setting_evaluation_criterion_gives_passed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(StudentAnswerTriangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }


        [Fact]
        public async Task evaluation_criteria_is_matched_to_the_correct_question()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);

            await kernel.SubmitCodeAsync(EvaluateRectangeArea("2"));

            await kernel.SubmitCodeAsync(EvaluateTriangleArea("3"));

            var resultQuestion2 = await kernel.SubmitCodeAsync(StudentAnswerRectangleArea("3"));

            var resultQuestion1 = await kernel.SubmitCodeAsync(StudentAnswerTriangleArea("2"));

            resultQuestion1.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);

            resultQuestion2.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<CodeEvaluationProduced>()
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
                .UseEvaluateMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(EvaluateTriangleArea());

            await kernel.SubmitCodeAsync(EvaluateAlwaysPassing());

            await kernel.SubmitCodeAsync(StudentAnswerRectangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }


        [Fact]
        public async Task only_evaluation_criterion_that_returns_a_boolean_can_affect_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseEvaluateMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(EvaluateAlwaysPassing());

            // a code snippet which does not return a value
            await kernel.SubmitCodeAsync(@"
#!evaluate 1

var x = area(10, 10) == 50;
");

            // a code snippet which returns not a bool
            await kernel.SubmitCodeAsync(@"
#!evaluate 1

500
");

            await kernel.SubmitCodeAsync(StudentAnswerRectangleArea());

            events.Should().ContainSingle<CodeEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }
    }
}

