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
        public static Task OnLoadAsync(Kernel kernel)
        {
            if (kernel is CompositeKernel compositeKernel)
            {
                compositeKernel.UseProgressiveLearning();
            }
            else
            {
                throw new Exception("Not composite kernel");
            }

            if (KernelInvocationContext.Current is { } context)
            {
                context.DisplayAs("Hello world! EducationExtension loaded!!!", "text/markdown");
            }
             
            return Task.CompletedTask;
        }

        public static void RegisterEvents()
        {
        }
    }
}
