using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        protected async Task<CompositeKernel> CreateKernel(LessonMode mode, HttpClient httpClient = null)
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel(),
                new FakeKernel("vscode")
            };

            Lesson.Mode = mode;

            await Main.OnLoadAsync(kernel, httpClient);

            return kernel;
        }

        protected string ToModelAnswer(string answer)
        {
            return $"#!model-answer\r\n{answer}";
        }

        protected string GetNotebookPath(string notebookName)
        {
            return PathUtilities.GetNotebookPath(notebookName);
        }
    }
}
