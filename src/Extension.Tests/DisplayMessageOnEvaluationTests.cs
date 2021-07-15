using FluentAssertions;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using HtmlAgilityPack;
using Extension;

namespace Extension.Tests
{
    public class DisplayMessageOnEvaluationTests
    {
        [Fact]
        public void passing_evaluation_will_produce_default_success_message()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetOutcome(Outcome.Success);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("Success: All tests passed.");
        }


        [Fact]
        public void failing_evaluation_will_produce_default_fail_message()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetOutcome(Outcome.Failure);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("Failure: Incorrect solution.");
        }

        [Fact]
        public void evaluation_will_produce_custom_message()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetOutcome(Outcome.Failure, "Try again.");

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("Failure: Try again.");
        }

        [Fact]
        public void evaluation_can_have_a_label()
        {
            // arrange
            var evaluation = new Evaluation("General case");
            evaluation.SetOutcome(Outcome.Failure, "Try again.");

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("[General case] Failure: Try again.");
        }

        [Fact]
        public void partially_correct_evaluation_will_produce_default_partial_success_evaluation()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetOutcome(Outcome.PartialSuccess);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("Partial Success: Some tests passed.");
        }

        [Fact]
        public void teacher_can_provide_hint()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetOutcome(Outcome.Failure, "Try again.", " Look over recursion.");

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div[@class='hint']");

            div.InnerText
                .Should()
                .Be(" Look over recursion.");
        }

        [Fact]
        public void teacher_can_provide_feedback_for_a_specific_rule()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetRuleOutcome("Code compiles", Outcome.Success, "Your submission has compiled.");
            evaluation.SetOutcome(Outcome.Success);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div[@class='rule']");

            div.InnerText
                .Should()
                .Be("[Code compiles] Success: Your submission has compiled.");
        }

        [Fact]
        public void display_correct_number_of_rules_for_partial_success()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetRuleOutcome("Code compiles", Outcome.Success);
            evaluation.SetRuleOutcome("Code matches output", Outcome.Success);
            evaluation.SetRuleOutcome("Code is recursive", Outcome.Failure);
            evaluation.SetOutcome(Outcome.PartialSuccess);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                    .Contain("(2/3)");
        }

        [Fact]
        public void display_correct_number_of_rules_for_success()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetRuleOutcome("Code compiles", Outcome.Success);
            evaluation.SetRuleOutcome("Code matches answer key", Outcome.Success);
            evaluation.SetOutcome(Outcome.Success);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);

            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                    .Contain("(2/2)");
        }

        [Fact]
        public void feedback_message_can_be_displayed()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.SetRuleOutcome("Code compiles", Outcome.Success, "No errors detected");
            evaluation.SetRuleOutcome("Runtime error", Outcome.Failure, "Time limit exceeded");
            evaluation.SetOutcome(Outcome.PartialSuccess);

            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var div = htmlDoc.DocumentNode
                .SelectSingleNode("//div");

            div.InnerText
                .Should()
                .Be("Partial Success (1/2): Some tests passed.\n[Runtime error] Failure: Time limit exceeded\n[Code compiles] Success: No errors detected\n");
        }

    }
}
