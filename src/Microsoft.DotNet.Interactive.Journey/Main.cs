using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Interactive.Journey
{
    public static class Main
    { 
        public static Task OnLoadAsync(Kernel kernel, HttpClient httpClient = null)
        {
            Lesson.Clear();
            if (kernel is CompositeKernel compositeKernel)
            {
                Lesson.ResetChallenge();
                compositeKernel.UseProgressiveLearning(httpClient)
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

            return Task.CompletedTask;
        }

        public static void RegisterEvents()
        {
        }
    }
}
