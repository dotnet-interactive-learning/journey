using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class ParsingTests : ProgressiveLearningTestBase
    {
        private string GetNotebookPath(string relativeFilePath)
        {
            var prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(prefix, relativeFilePath));
        }

        [Fact]
        public async Task parser_can_parse_teacher_notebook_with_two_challenges_with_all_components_defined()
        {
            var file = new FileInfo(GetNotebookPath(@"Notebooks\forParsing1.dib"));
            var rawData = await File.ReadAllBytesAsync(file.FullName);
            var document = NotebookFileFormatHandler.Parse(file.Name, rawData, "csharp", new Dictionary<string, string>());

            var lesson = NotebookLessonParser.Parse(document);

            lesson.Setup.Select(sc => sc.Code).Join("\r\n").Should().ContainAll("lessonSetupCell1", "lessonSetupCell2");

            lesson.Challenges[0].Name.Should().Be("Challenge1Name");

            lesson.Challenges[0].Setup.Select(sc => sc.Code).Join("\r\n")
                .Should().ContainAll("challenge1SetupCell1", "challenge1SetupCell2");

            lesson.Challenges[0].EnvironmentSetup.Select(sc => sc.Code).Join("\r\n")
                .Should().ContainAll("challenge1EnvironmentSetupCell1", "challenge1EnvironmentSetupCell2");

            lesson.Challenges[0].Contents.Select(sec => sec.Code).Join("\r\n")
                .Should().ContainAll("challenge1QuestionCell1", "challenge1QuestionCell2");

            lesson.Challenges[1].Name.Should().Be("Challenge2Name");

            lesson.Challenges[1].Setup.Select(sc => sc.Code).Join("\r\n")
                .Should().ContainAll("challenge2SetupCell1", "challenge2SetupCell2");

            lesson.Challenges[1].EnvironmentSetup.Select(sc => sc.Code).Join("\r\n")
                .Should().ContainAll("challenge2EnvironmentSetupCell1", "challenge2EnvironmentSetupCell2");

            lesson.Challenges[1].Contents.Select(sec => sec.Code).Join("\r\n")
                .Should().ContainAll("challenge2QuestionCell1", "challenge2QuestionCell2");
        }


        [Fact]
        public async Task duplicate_challenge_name_causes_parser_to_throw_exception()
        {
            var file = new FileInfo(GetNotebookPath(@"Notebooks\forParsing2DuplicateChallengeName.dib"));
            var rawData = await File.ReadAllBytesAsync(file.FullName);
            var document = NotebookFileFormatHandler.Parse(file.Name, rawData, "csharp", new Dictionary<string, string>());

            Action parsingDuplicateChallengeName = () => NotebookLessonParser.Parse(document);

            parsingDuplicateChallengeName
                .Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("conflicts");
        }

        [Fact]
        public async Task notebook_with_no_challenge_causes_parser_to_throw_exception()
        {
            var file = new FileInfo(GetNotebookPath(@"Notebooks\noChallenge.dib"));
            var rawData = await File.ReadAllBytesAsync(file.FullName);
            var document = NotebookFileFormatHandler.Parse(file.Name, rawData, "csharp", new Dictionary<string, string>());

            Action parsingDuplicateChallengeName = () => NotebookLessonParser.Parse(document);

            parsingDuplicateChallengeName
                .Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("This lesson has no challenges");
        }

        [Fact]
        public async Task a_challenge_with_no_question_causes_parser_to_throw_exception()
        {
            var file = new FileInfo(GetNotebookPath(@"Notebooks\challengeWithNoQuestion.dib"));
            var rawData = await File.ReadAllBytesAsync(file.FullName);
            var document = NotebookFileFormatHandler.Parse(file.Name, rawData, "csharp", new Dictionary<string, string>());

            Action parsingDuplicateChallengeName = () => NotebookLessonParser.Parse(document);

            parsingDuplicateChallengeName
                .Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("empty question");
        }
    }
}
