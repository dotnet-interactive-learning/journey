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
        public void passing_evaluation_gives_correct_display_string()
        {
            // arrange
            var evaluation = new Evaluation();
            evaluation.Passed = true;
            // act
            var message = evaluation.ToDisplayString(HtmlFormatter.MimeType);
            // assert
            message
                .Should()
                .Contain("Success");
        }


        [Fact]
        public void 


    }
}
