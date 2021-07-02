using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Events;

namespace Extension
{
    public class Evaluator
    {
        SortedDictionary<int, string> executionCriteria;

        public Evaluator()
        {
            executionCriteria = new SortedDictionary<int, string>();
        }

        public Evaluation EvaluateResult(KernelCommandResult result)
        {
            var events = result.KernelEvents.ToEnumerable();
            bool errorExists = events.Any(e => e is ErrorProduced || e is CommandFailed);
            if (errorExists)
            {
                return new Evaluation { Passed = false };

            }
            return new Evaluation { Passed = true };
        }

        public void AddInputExecutionCriterion(int cellId, string code)
        {
            executionCriteria.Remove(cellId);
            executionCriteria.Add(cellId, code);
        }

        public string GetInputExecutionCriterion(int cellId)
        {
            if (executionCriteria.ContainsKey(cellId))
            {
                return executionCriteria[cellId];
            }
            return "";
        }

        public Evaluation EvaluateInputExecution(KernelCommandResult result)
        {
            var events = result.KernelEvents.ToEnumerable();
            var foundEvent = events.FirstOrDefault(e => e is ReturnValueProduced);
            if (foundEvent is ReturnValueProduced returnValueProduced
                && returnValueProduced.Value is bool criteriaResult)
            {
                if (!criteriaResult)
                {
                    return new Evaluation { Passed = false };
                }
            }
            return new Evaluation { Passed = true };
        }
    }
}