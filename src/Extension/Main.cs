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
            }
            else
            {
                throw new Exception("Not composite kernel");
            }

            if (kernel.RootKernel.FindKernel("csharp") is DotNetKernel dotNetKernel)
            {
                await kernel.SubmitCodeAsync($"#r \"{typeof(Lesson).Assembly.Location}\"");
                await kernel.SubmitCodeAsync($"#r \"{typeof(Lesson).Namespace}\"");
                await dotNetKernel.SetVariableAsync<Lesson>("Lesson", lesson);
            }
            else
            {
                throw new Exception("Not dotnet kernel");
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
