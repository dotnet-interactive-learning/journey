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
    public class CodeRunEvaluationTests
    {
        [Fact]
        public async Task submitting_code_that_fails_evaluation_criterion_gives_failed_evaluation()
        {
            var evaluator = new Evaluator();       
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            // instructor provides testcase script which returns bool
            await kernel.SubmitCodeAsync(@"
#!answer 5
var testcases = new (double, double, double)[]
{
    (0, 0, 0),
    (0.5, 1.0, 0.25),
    (100, 50, 2500)
};

testcases.All(testcase => triangleArea(testcase.Item1, testcase.Item2) == testcase.Item3)
");

            // students code runs
            await kernel.SubmitCodeAsync(@"
#!question 5

double triangleArea(double x, double y) 
{
    return x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
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
                .UseAnswerMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            // instructor provides testcase script which returns bool
            await kernel.SubmitCodeAsync(@"
#!answer 6
var testcases = new (double, double, double)[]
{
    (0, 0, 0),
    (0.5, 1.0, 0.25),
    (100, 50, 2500)
};

testcases.All(testcase => triangleArea(testcase.Item1, testcase.Item2) == testcase.Item3)
");

            // students code runs
            await kernel.SubmitCodeAsync(@"
#!question 6

double triangleArea(double x, double y) 
{
    return 0.5 * x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }


        [Fact]
        public async Task forward_symbol_declaration_will_not_affect_evaluation_criteria_runs()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(@"
double triangleArea(double x, double y) => 0.0
");

            await kernel.SubmitCodeAsync(@"
#!answer 6
var testcases = new (double, double, double)[]
{
    (0, 0, 0),
    (0.5, 1.0, 0.25),
    (100, 50, 2500)
};

testcases.All(testcase => triangleArea(testcase.Item1, testcase.Item2) == testcase.Item3)
");

            // students code runs
            await kernel.SubmitCodeAsync(@"
#!question 6

double triangleArea(double x, double y) 
{
    return 0.5 * x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }

        [Fact]
        public async Task submitting_code_without_setting_evaluation_criterion_gives_passed_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);


            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(@"
#!question 5

double triangleArea(double x, double y) 
{
    return 0.5 * x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
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
                .UseAnswerMagicCommand(evaluator);

            await kernel.SubmitCodeAsync(@"
#!answer 1

area(10, 10) == 100
");

            await kernel.SubmitCodeAsync(@"
#!answer 2

area(10, 10) == 50
");

            var resultQuestion2 = await kernel.SubmitCodeAsync(@"
#!question 2

double area(double x, double y) 
{
    return x * y;
}
");

            var resultQuestion1 = await kernel.SubmitCodeAsync(@"
#!question 1

double area(double x, double y) 
{
    return 0.5 * x * y;
}
");

            resultQuestion1.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<CodeRunEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);

            resultQuestion2.KernelEvents.ToSubscribedList()
                .Should().ContainSingle<CodeRunEvaluationProduced>()
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

            await kernel.SubmitCodeAsync(@"
#!answer 1

area(10, 10) == 50
");

            await kernel.SubmitCodeAsync(@"
#!answer 1

area(0, 0) == 0
");

            await kernel.SubmitCodeAsync(@"
#!question 1

double area(double x, double y) 
{
    return x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(false);
        }


        [Fact]
        public async Task evaluation_criterion_that_does_not_have_ReturnValueProduced_does_not_affect_evaluation()
        {
            var evaluator = new Evaluator();
            using var csharpKernel = new CSharpKernel();
            using var kernel = new CompositeKernel { csharpKernel };
            kernel
                .UseQuestionMagicCommand(evaluator)
                .UseAnswerMagicCommand(evaluator);

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SubmitCodeAsync(@"
#!answer 1

area(0, 0) == 0
");

            await kernel.SubmitCodeAsync(@"
#!answer 1

var x = area(10, 10) == 50;
");

            await kernel.SubmitCodeAsync(@"
#!question 1

double area(double x, double y) 
{
    return x * y;
}
");

            events.Should().ContainSingle<CodeRunEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }
    }
}

