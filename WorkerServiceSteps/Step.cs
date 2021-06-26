using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public class Step<T> where T : BaseBufferItem
    {
        // Holds data for processing
        public BlockingCollection<T> Buffer { get; }
        // Action we want to perform on each item
        public Func<T, Task<T>> StepActionAsync { get; }
        // Should retry in case of failure?
        public bool ShouldRetry { get; set; }
        public int MaxRetryCount { get; set; }
        // Number of threads which will be running to process this step
        public int DegreeOfParallelism { get; set; } = 1;

        public Step(int bufferSize, Func<T, Task<T>> stepActionAsync)
        {
            Buffer = new BlockingCollection<T>(bufferSize);
            StepActionAsync = stepActionAsync;
        }
    }
}
