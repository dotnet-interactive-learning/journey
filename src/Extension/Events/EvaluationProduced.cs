using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Events
{
    public class EvaluationProduced : KernelEvent
    {
        public Evaluation Evaluation { get; }

        public EvaluationProduced(
            KernelCommand command,
            Evaluation evaluation) : base(command)
        {
            Evaluation = evaluation;
        }
    }
}

