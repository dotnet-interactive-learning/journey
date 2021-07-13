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
        [Fact(Skip = "later")]
        public async Task output_with_error_event_produces_failed_evaluation()
        {
            //arrange
            using var csharpkernel = new CSharpKernel();
            using var events = csharpkernel.KernelEvents.ToSubscribedList();
            var result = await csharpkernel.SubmitCodeAsync("bdobnridf");

            //act
            //var evaluation = new Evaluator().EvaluateResult(result);

            //assert
            //evaluation.Passed.Should().Be(false);


        }

        [Fact(Skip = "later")]
        public async Task when_the_output_passes_all_rules_then_evaluation_passes()
        {
            //in english:
            //If the users output is not the same as the output the teacher(or notebook creater) expects
            //then there should be an error.

            //possible format:
            //set var for submission code output
            //set var for expected criteria
            //if they are the same then this test passes

//            //arrange
//            using var csharpkernel = new CSharpKernel();
//            using var events = csharpkernel.KernelEvents.ToSubscribedList();
//            var result = await csharpkernel.SubmitCodeAsync(
//@"//return 1+1
//1+1");

//            //act
//            var evaluator = new Evaluator();

//            evaluator.AddRule(new Rule(r =>
//            {
               
//            }));
//            var evaluation = evaluator.EvaluateResult(result);
//            //I think the following code should return the professors feedback based on a pass or fail
//            //so I have to have a variable containing the message like: var proffessorsPassedFeedback = ... and var proffessorsFailedFeedback = ...
//            //then I can call them below 
//            //Question: If I do this should I call an exception or return true/false, something to think about
//            //if (evaluation.Equals(true))
//            //{
//            //    return true;
//            //}
//            //else { throw new Exception(professorsFailedFeedback) }

//            //assert
//            evaluation.Passed.Should().Be(true);


            //throw new Exception();

        }


        [Fact]
        public async Task when_the_output_fails_any_rule_then_evaluation_fails()
        {
            //in english:
            //If the users output is not the same as the output the teacher(or notebook creater) expects
            //then there should be an error.

            //possible format:
            //set var for submission code output
            //set var for expected criteria
            //if they are the same then this test passes

            //arrange

            var banana = new Banana();
            //banana.Passed

            using var csharpkernel = new CSharpKernel();
            using var events = csharpkernel.KernelEvents.ToSubscribedList();
            var result = await csharpkernel.SubmitCodeAsync(
@"//return 2
1+2");

            //act
            var evaluator = new Evaluator();

            evaluator.AddRule(new Rule(r => r.Fail()));
            var evaluation = evaluator.EvaluateResult(banana);


            //assert
            evaluation.Passed.Should().Be(false);




        }

        [Fact(Skip = "later")]
        public async Task no_submission_recieved()
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



//            //arrange
//            using var csharpkernel = new CSharpKernel();
//            using var events = csharpkernel.KernelEvents.ToSubscribedList();
//            var result = await csharpkernel.SubmitCodeAsync(
//@"//return 1+1
//1+1");

//            //act
//            if (result == null)
//            {
//                throw new Exception("Submission empty");

//            }

//            //assert

//            throw new Exception();



        }


        [Fact]
        public void when_banana_fail_is_called_then_banana_passed_is_false_()
        {
            var banana = new Banana();
            banana.Fail();
            
            banana.Passed.Should().Be(false);
        }


        [Fact]
        public void when_banana_pass_is_called_banana_passed_is_true()
        {
            var banana = new Banana();
            banana.Pass();

            banana.Passed.Should().Be(true);
        }
    }
}
