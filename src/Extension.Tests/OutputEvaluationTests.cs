using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Xunit;

namespace Extension.Tests
{
    public class OutputEvaluationTests
    {
        [Fact]
        public async Task output_with_error_event_produces_failed_evaluationAsync()
        {
            //arrange
            using var csharpkernel = new CSharpKernel();
            using var events = csharpkernel.KernelEvents.ToSubscribedList();
            var result = await csharpkernel.SubmitCodeAsync("bdobnridf");

            //act
            var evaluation = new Evaluator().EvaluateResult(result);


            //assert
            evaluation.Outcome.Should().Be(Outcome.Failure);
        }
    }
}
