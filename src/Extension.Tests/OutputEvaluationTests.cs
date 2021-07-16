using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
        public void output_with_error_event_produces_failed_evaluation()
        {
            //in english:
            //If the users output is not the same as the output the teacher(or notebook creater) expects
            //then there should be an error.

            //possible format:
            //set var for submission code output
            //set var for expected criteria
            //if they are the same then this test passes

            //arrange

            //            var banana = new RuleContext();
            //            //banana.Passed

            //            using var csharpkernel = new CSharpKernel();
            //            using var events = csharpkernel.KernelEvents.ToSubscribedList();
            //            var result = await csharpkernel.SubmitCodeAsync(
            //@"//return 2
            //1+2");

            //act
            //var evaluation = new Evaluator().EvaluateResult(result);

            //assert
            //evaluation.Passed.Should().Be(false);

            throw new NotImplementedException();

        }

        [Fact(Skip = "later")]
        public void no_submission_recieved()
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

            throw new NotImplementedException();

        }


        [Fact]
        public void when_ruleContext_fail_is_called_then_ruleContext_passed_is_false_()
        {
            var challengeContext = new ChallengeContext(new Challenge(Enumerable.Empty<EditableCode>().ToList()));
            var ruleContext = new RuleContext(challengeContext);
            ruleContext.Fail();
            
            ruleContext.Passed.Should().Be(false);
        }


        [Fact]
        public void when_ruleContext_pass_is_called_ruleContext_passed_is_true()
        {
            var challengeContext = new ChallengeContext(new Challenge(Enumerable.Empty<EditableCode>().ToList()));
            var ruleContext = new RuleContext(challengeContext);
            ruleContext.Pass();

            ruleContext.Passed.Should().Be(true);
        }
    }
}
