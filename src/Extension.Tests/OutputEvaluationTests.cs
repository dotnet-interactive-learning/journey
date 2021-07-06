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
            evaluation.Passed.Should().Be(false);
            

        }

        [Fact]
        public async Task output_that_matches_the_expected_criteria()
        {
            //in english:
            //If the users output is not the same as the output the teacher(or notebook creater) expects
            //then there should be an error.

            //possible format:
            //set var for submission code output
            //set var for expected criteria
            //if they are the same then this test passes
            //arrange

            //act


            //assert
            throw new Exception();

        }

        [Fact]
        public async Task no_output_recieved()
        {
            //ideas in english:
            //-would it be wrong to return an error if the output is empty bc what if the submitted code doesn't
            //have any return values
            //-I think in cases where there should be something in the output then this would be useful 

            //So a possible format can be:
            //1.check if the output should be empty (according to teacher)
            //2.if it should be empty and it is empty:
            //then no error should be produced (this question would only be evaluated on the users submission which is a different senario)
            //3. if it shouldn't be empty and it is empty:
            //then we should generate an error

            //^Is this too much to put in one test, should these be seperate test cases


            //arrange


            //act


            //assert


            throw new Exception();


        }

    }
}
