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
            var lesson = new Lesson();
            lesson.IsTeacherMode = true;
            if (kernel is CompositeKernel compositeKernel)
            {
                compositeKernel.UseProgressiveLearning(lesson)
                    .UseModelAnswerValidation(lesson);
                await compositeKernel.Bootstrapping(lesson);
            }
            else
            {
                throw new Exception("Not composite kernel");
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
