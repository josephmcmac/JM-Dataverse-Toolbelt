using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JosephM.Core.Utility
{
    public static class ParallelTaskHelper
    {
        public static void RunParallelTasks(Action parallelAction, int processCount)
        {
            var parallelTasks = new List<Task>();
            for (var i = 0; i < processCount; i++)
            {
                parallelTasks.Add(Task.Run(parallelAction));
            }
            var waitTask = Task.WhenAll(parallelTasks);
            waitTask.Wait();
            if (waitTask.Status == TaskStatus.Canceled)
            {
                throw new InvalidOperationException("Parallel task await unexpectedly cancelled", waitTask.Exception);
            }
            if (waitTask.Status == TaskStatus.Faulted)
            {
                throw new InvalidOperationException("Parallel task await unexpectedly faulted", waitTask.Exception);
            }
        }
    }
}
