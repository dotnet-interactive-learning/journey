using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.CommandLine;
using Xunit;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Extension.Tests.Utilities;
using Microsoft.DotNet.Interactive.Events;
using System.Linq;

namespace Extension.Tests
{
    public class InputEvaluationTests
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

            // instructor provides dummy symbols
            await kernel.SubmitCodeAsync(@"
double triangleArea(double x, double y) => 0
");
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

            events.Should().ContainSingle<ExecutionEvaluationProduced>()
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

            // instructor provides dummy symbols
            await kernel.SubmitCodeAsync(@"
double triangleArea(double x, double y) => 0
");
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
    return 0.5 * x * y;
}
");

            events.Should().ContainSingle<ExecutionEvaluationProduced>()
                .Which.Evaluation.Passed.Should().Be(true);
        }
    }
}
