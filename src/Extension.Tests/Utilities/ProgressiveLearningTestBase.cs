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

        protected async Task<CompositeKernel> CreateBootstrappedKernel(Lesson lesson, bool isTeacherMode)
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel(),
                new FakeKernel("vscode")
            };

            lesson.IsTeacherMode = isTeacherMode;
            await kernel.Bootstrapping(lesson);
            kernel.UseProgressiveLearning(lesson)
                .UseModelAnswerValidation();

            kernel.DefaultKernelName = "csharp";
            return kernel;
        }

        protected CompositeKernel CreateKernel(Lesson lesson = null)
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel(),
                new FakeKernel("vscode")
            };

            if (lesson is not null)
            {
                kernel.UseProgressiveLearning(lesson);
            }
            else
            {
                kernel.UseProgressiveLearning();
            }

            kernel.DefaultKernelName = "csharp";
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
