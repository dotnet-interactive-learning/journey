using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected CompositeKernel CreateKernel(Lesson lesson)
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel(),
                new FakeKernel("vscode")
            }.UseProgressiveLearning(lesson);
            kernel.DefaultKernelName = "csharp";
            return kernel;
        }
    }
}
