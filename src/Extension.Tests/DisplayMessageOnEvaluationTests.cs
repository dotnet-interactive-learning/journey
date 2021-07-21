using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.DotNet.Interactive.Formatting;
using Xunit;

namespace Extension.Tests
{

    public class RuleEvaluationFormattingTests
    {
        [Fact]
        public void passing_rule_evaluation_will_produce_default_success_summary()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Success);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/summary");

            summary.InnerText
                .Should()
                .Be("Success");
        }

        [Fact]
        public void passing_rule_evaluation_will_produce_default_success_message()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Success);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/p");

            summary.InnerText
                .Should()
                .Be("All tests passed.");
        }

        [Fact]
        public void failing_rule_evaluation_will_produce_default_fail_summary()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Failure);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/summary");

            summary.InnerText
                .Should()
                .Be("Failure");
        }

        [Fact]
        public void failing_rule_evaluation_will_produce_default_fail_message()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Failure);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/p");

            summary.InnerText
                .Should()
                .Be("Incorrect solution.");
        }

        [Fact]
        public void rule_evaluation_will_produce_custom_message()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Failure, reason: "Try again.");

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var p = htmlDoc.DocumentNode
                .SelectSingleNode("//details/p");

            p.InnerText
                .Should()
                .Be("Try again.");
        }

        [Fact]
        public void rule_evaluation_can_have_a_label()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Failure, "General case", "Try again.");

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/summary");

            summary.InnerText
                .Should()
                .Be("[ General case ] Failure");
        }

        [Fact]
        public void partially_correct_rule_evaluation_will_produce_default_partial_success_summary()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.PartialSuccess);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var summary = htmlDoc.DocumentNode
                .SelectSingleNode("//details/summary");

            summary.InnerText
                .Should()
                .Be("Partial Success");
        }

        [Fact]
        public void partially_correct_rule_evaluation_will_produce_default_partial_success_message()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.PartialSuccess);

            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(message);

            var p = htmlDoc.DocumentNode
                .SelectSingleNode("//details/p");

            p.InnerText
                .Should()
                .Be("Some tests passed.");
        }

        [Fact]
        public void teacher_can_provide_hint_to_rule_evaluation()
        {
            // arrange
            var evaluation = new RuleEvaluation(Outcome.Failure, null, "Try again.", " Look over recursion.");

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
    }
}
