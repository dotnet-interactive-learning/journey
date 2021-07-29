using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public static class Main
    { 
        public static async Task OnLoadAsync(Kernel kernel)
        {
            Lesson.Clear();
            if (kernel is CompositeKernel compositeKernel)
            {
                Lesson.ResetChallenge();
                compositeKernel.UseProgressiveLearning()
                    .UseProgressiveLearningMiddleware()
                    .UseModelAnswerValidation();
            }
            else
            {
                throw new ArgumentException("Not composite kernel");
            }

            if (KernelInvocationContext.Current is { } context)
            {
                context.DisplayAs("Hello world! EducationExtension loaded!", "text/markdown");
            }
        }

        public static void RegisterEvents()
        {
        }
    }
}
