using FluentAssertions;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class DisplayMessageOnEvaluationTests
    {
        [Fact]
        public void passing_evaluation_will_produce_success_message()
        {
            // arrange
            var evaluation = new Evaluation(true);
            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            message
                .Should()
                .Contain("Success");
        }


        [Fact]
        public void failing_evaluation_will_produce_fail_message()
        {
            // arrange
            var evaluation = new Evaluation(false);
            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            message
                .Should()
                .Contain("Fail");
        }

        [Fact]
        public void evaluation_will_produce_custom_message()
        {
            // arrange
            var evaluation = new Evaluation(false, "Try again");
            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            message
                .Should()
                .Contain("Try again");
        }

        /*        [Fact]
                public void display_string_contains_unsupported_character()
                {

                }

                [Fact]
                public void display_string_matches_evaluation_string()
                {
                    // does not matter if evaluation had passed

                    var expectedString = "example";

                    Assert.Equal(expectedString, actualString);
                }

                [Fact]
                public void evaluation_is_correctly_displayed_as_a_hint()
                {
                    // arrange
                    var evaluation = new Evaluation();
                    evaluation.Passed = false;

                    // act
                    var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);

                    command
                        .Should()
                        .Be("#!hint");
                }

                [Fact]
                public void hint_displays_when_clicked_on()
                {

                }

                [Fact]
                public void display_string_is_correctly_displayed_as_a_hint()
                {

                }*/



    }
}
