using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public sealed class Pipeline<T> where T : BaseBufferItem
    {
        private List<Step<T>> _steps;
        private List<Task> _tasks;

        public event Func<T, Task> OnFinishAsync;

        public Pipeline()
        {
            _steps = new List<Step<T>>();
            _tasks = new List<Task>();
        }

        public void AddStep(Step<T> step)
        {
            _steps.Add(step);
        }

        public bool AddItem(T item)
        {
            var firstStep = _steps[0];
            if (!firstStep.Buffer.IsAddingCompleted)
            {
                try
                {
                    firstStep.Buffer.Add(item);
                    ItemsSummery.IncrementNumberOfTotalItemsEnequed();
                    return true;
                }
                catch
                {
                    Console.WriteLine("unable to add item to pipeline");
                }
            }
            return false;
        }

        public void StartPipeline()
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                int localStepIndex = i;
                for (int degreeOfParallelism = 0; degreeOfParallelism < _steps[i].DegreeOfParallelism; degreeOfParallelism++)
                {
                    var task = Task.Run(async () => await StartStepAsync(localStepIndex));
                    _tasks.Add(task);
                }
            }
        }

        private async Task StartStepAsync(int stepIndex)
        {
            int numberOfSteps = _steps.Count;
            var step = _steps[stepIndex];
            var isFinalStep = (stepIndex == (numberOfSteps - 1));

            foreach (var input in step.Buffer.GetConsumingEnumerable())
            {
                try
                {
                    var output = await step.StepActionAsync(input);

                    if (!isFinalStep)
                    {
                        var nextStep = _steps[stepIndex + 1];
                        nextStep.Buffer.Add(output);
                    }
                    else
                    {
                        if (OnFinishAsync != null)
                            await OnFinishAsync(output);

                        ItemsSummery.IncrementNumberOfItemsSuccessfullyProcessed();
                    }
                }
                catch
                {
                    if (step.ShouldRetry && step.MaxRetryCount > input.RetryCount)
                    {
                        input.RetryCount++;
                        step.Buffer.Add(input);
                    }
                    else
                    {
                        ItemsSummery.IncrementNumberOfItemsFailedToProcess();
                    }
                }
            }
        }

        public async Task StopPipelineAsync()
        {
            _steps[0].Buffer.CompleteAdding();
            while (!ItemsSummery.AreAllItemsProcessed())
            {
                await Task.Delay(1000);
            }
            _steps.ForEach(x => x.Buffer.CompleteAdding());
            await Task.WhenAll(_tasks);
        }
    }
}
