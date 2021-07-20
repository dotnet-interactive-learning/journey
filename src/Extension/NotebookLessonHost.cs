using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class NotebookLessonHost : ILessonHost
    {
        public Kernel Kernel { get; }
        public NotebookLessonHost(Kernel kernel,Lesson lesson)
        {
            Kernel = kernel;
            lesson.Host = this;
        }
        public async Task StartChallengeAsync(Challenge challenge)
        {
            foreach (var code in challenge.Contents)
            {
                await Kernel.SendAsync(new SendEditableCode(code.Language, code.Code));
            }
        }
    }
}
