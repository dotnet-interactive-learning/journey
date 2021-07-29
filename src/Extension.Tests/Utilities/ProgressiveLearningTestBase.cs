using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public abstract class ProgressiveLearningTestBase
    {
        protected Challenge GetEmptyChallenge()
        {
            return new Challenge();
        }

        protected async Task<CompositeKernel> CreateKernel(LessonMode mode = LessonMode.StudentMode)
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel(),
                new FakeKernel("vscode")
            };

            await Main.OnLoadAsync(kernel);

            Lesson.Mode = mode;

            return kernel;
        }

        protected string ToModelAnswer(string answer)
        {
            return $"#!model-answer\r\n{answer}";
        }

        protected string GetNotebookPath(string relativeFilePath)
        {
            var prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(prefix, relativeFilePath));
        }
    }
}
