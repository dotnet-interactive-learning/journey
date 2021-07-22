using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Notebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extension
{
    public enum LessonDirective
    {
        Challenge
    }

    public enum ChallengeDirective
    {
        ChallengeSetup,
        Question,
        Scratchpad
    }

    public class NotebookLessonParser
    {
        private static Dictionary<string, LessonDirective> _stringToLessonDirectiveMap = new Dictionary<string, LessonDirective>
        {
            { "Challenge", LessonDirective.Challenge }
        };

        private static Dictionary<string, ChallengeDirective> _stringToChallengeDirectiveMap = new Dictionary<string, ChallengeDirective>
        {
            { "ChallengeSetup", ChallengeDirective.ChallengeSetup },
            { "Question", ChallengeDirective.Question },
            { "Scratchpad", ChallengeDirective.Scratchpad }
        };

        public static Lesson Parse(NotebookDocument document)
        {
            List<NotebookCell> lessonSetup = new();
            List<List<NotebookCell>> challenges = new();
            List<string> challengeNames = new();

            int indexOfFirstLessonDirective = 0;

            while (indexOfFirstLessonDirective < document.Cells.Length 
                && !TryParseLessonDirectiveCell(document.Cells[indexOfFirstLessonDirective], out var _, out var _, out var _))
            {
                lessonSetup.Add(document.Cells[indexOfFirstLessonDirective]);
                indexOfFirstLessonDirective++;
            }

            List<NotebookCell> currentChallenge = new();
            int cellCount = document.Cells.Length;
            for (int i = indexOfFirstLessonDirective; i < cellCount;)
            {
                if (TryParseLessonDirectiveCell(document.Cells[i], out var remainingCell, out var _, out var challengeName))
                {
                    if (!string.IsNullOrWhiteSpace(remainingCell.Contents))
                    {
                        currentChallenge.Add(remainingCell);
                    }
                    challengeNames.Add(challengeName);
                    i++;
                }
                else
                {
                    while (i < cellCount && !TryParseLessonDirectiveCell(document.Cells[i], out var _, out var _, out var _))
                    {
                        currentChallenge.Add(document.Cells[i]);
                        i++;
                    }
                    challenges.Add(currentChallenge);
                    currentChallenge = new();
                }
            }
            challenges.Add(currentChallenge);

            var lesson = new Lesson();

            // todo: lesson setup

            HashSet<string> challengeNamesSet = new();
            var index = 1;
            foreach (var item in challengeNames.Zip(challenges))
            {
                var name = item.Item1;
                var challengeCells = item.Item2;

                name = string.IsNullOrWhiteSpace(name) ? $"Challenge {index}" : name;
                if (!challengeNamesSet.Add(name))
                {
                    throw new ArgumentException($"{name} conflicts with an existing challenge name");
                }

                var challenge = ParseChallenge(challengeCells, name);
                lesson.AddChallenge(challenge);

                index++;
            }

            return lesson;
        }

        private static Challenge ParseChallenge(List<NotebookCell> cells, string name)
        {
            var challenge = new Challenge();

            List<NotebookCell> challengeDefinition = new();
            List<NotebookCell> challengeSetup = new();
            List<NotebookCell> challengeContent = new();

            int indexOfFirstChallengeDirective = 0;

            while (indexOfFirstChallengeDirective < cells.Count
                && !TryParseChallengeDirectiveCell(cells[indexOfFirstChallengeDirective], out var _, out var directive, out var _))
            {
                challengeDefinition.Add(cells[indexOfFirstChallengeDirective]);
                indexOfFirstChallengeDirective++;
            }

            // todo: execute challenge definitions

            string currentDirective = null;
            for (int i = indexOfFirstChallengeDirective; i < cells.Count;)
            {
                if (TryParseChallengeDirectiveCell(cells[i], out var remainingCell, out var directive, out var _))
                {
                    currentDirective = directive;
                    if (!string.IsNullOrWhiteSpace(remainingCell.Contents))
                    {
                        AddChallengeComponent(currentDirective, remainingCell); 
                    }
                    i++;
                }
                else
                {
                    while (i < cells.Count && !TryParseChallengeDirectiveCell(cells[i], out var _, out var _, out var _))
                    {
                        AddChallengeComponent(currentDirective, cells[i]);
                        i++;
                    }
                }
            }

            challenge.Contents = challengeContent.Select(c => new SendEditableCode(c.Language, c.Contents)).ToList();
            challenge.ChallengeSetup = challengeSetup.Select(c => new SubmitCode(c.Language, c.Contents)).ToList();

            if (challenge.Contents.Count == 0)
            {
                throw new ArgumentException($"{challenge.Name} has an empty question");
            }

            return challenge;

            void AddChallengeComponent(string directiveName, NotebookCell cell)
            {
                ChallengeDirective directive = _stringToChallengeDirectiveMap[directiveName];
                switch (directive)
                {
                    case ChallengeDirective.ChallengeSetup:
                        challengeSetup.Add(cell);
                        break;
                    case ChallengeDirective.Question:
                        challengeContent.Add(cell);
                        break;
                    default:
                        break;
                }
            }
        }

        private static bool TryParseLessonDirectiveCell(NotebookCell cell, out NotebookCell remainingCell, out string directive, out string afterDirective)
        {
            if (!TryParseDirectiveCell(cell, out directive, out afterDirective, out remainingCell))
            {
                return false;
            }
            var isDirectiveValid = _stringToLessonDirectiveMap.Keys.Contains(directive);
            if (!isDirectiveValid)
            {
                throw new ArgumentException($"{directive} is an invalid directive");
            }
            return true;
        }

        private static bool TryParseChallengeDirectiveCell(NotebookCell cell, out NotebookCell remainingCell, out string directive, out string afterDirective)
        {
            if (!TryParseDirectiveCell(cell, out directive, out afterDirective, out remainingCell))
            {
                return false;
            }
            var isDirectiveValid = _stringToChallengeDirectiveMap.Keys.Contains(directive);
            if (!isDirectiveValid)
            {
                throw new ArgumentException($"{directive} is an invalid directive");
            }
            return true;
        }

        private static bool TryParseDirectiveCell(NotebookCell cell, out string directive, out string afterDirective, out NotebookCell remainingCell)
        {
            directive = null;
            afterDirective = null;
            remainingCell = null;

            if (cell.Language != "markdown")
            {
                return false;
            }

            var result = Regex.Split(cell.Contents, "\r\n|\r|\n");

            string directivePattern = @"[^\[]\[(?<directive>[a-zA-z]+)\][ ]?(?<afterDirective>[\S]*)";
            var match = Regex.Match(result[0], directivePattern);
            if (!match.Success)
            {
                return false;
            }

            directive = match.Groups["directive"].Value;
            afterDirective = match.Groups["afterDirective"]?.Value;
            remainingCell = new NotebookCell(cell.Language, string.Join(Environment.NewLine, result.Skip(1)));
            return true;
        }
    }
}
