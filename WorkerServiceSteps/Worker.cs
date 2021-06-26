using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Pipeline<BufferItem> _pipeline;
        private readonly CancellationTokenSource _cancellationTokenSource;

        List<string> badWords = new List<string> { "Kill", "Murder", "Fuck" };
        string[] sampleText = new string[]
            {
                "You are smart",
                "I will Kill you",
                "Get the fuck out of here",
                "I love food"
            };


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            _pipeline = GetPipeline();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();
            _pipeline.StartPipeline();
            int id = 0;
            //Add items to the pipeline until service is stopped
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(500);
                id++;
                BufferItem item = new BufferItem();
                item.ItemId = id;
                // adding a random text from samples
                item.Text = sampleText[random.Next(0, sampleText.Length - 1)];
                if (_pipeline.AddItem(item))
                    _logger.LogInformation($"Created item {id}");

                await Task.Delay(500);                
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await _pipeline.StopPipelineAsync();
            PrintSummery();
        }

        private Pipeline<BufferItem> GetPipeline()
        {
            Pipeline<BufferItem> pipeline = new Pipeline<BufferItem>();

            //Step 1
            Func<BufferItem, Task<BufferItem>> findBadWords = async (BufferItem input) =>
            {
                await Task.Delay(1000);
                if (badWords.Any(x => input.Text.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    input.ContainsBadWords = true;
                }

                _logger.LogInformation($"Processed step one for item {input.ItemId} ThreadID {Thread.CurrentThread.ManagedThreadId}");
                return input;
            };
            Step<BufferItem> stepOne = new Step<BufferItem>(10, findBadWords) { DegreeOfParallelism = 2 };
            pipeline.AddStep(stepOne);

            //Step 2
            Func<BufferItem, Task<BufferItem>> funcTwo = async (BufferItem input) =>
            {
                await Task.Delay(1000);
                // Write logic here to call an api
                _logger.LogInformation($"Calling API to update status of itemid: {input.ItemId}, text: \"{input.Text}\" in database, Contains bad words: {input.ContainsBadWords}");
                _logger.LogInformation($"Processed step two for item {input.ItemId} ThreadID {Thread.CurrentThread.ManagedThreadId} CurrentProcessorId {Thread.GetCurrentProcessorId()}");
                return input;
            };
            Step<BufferItem> stepTwo = new Step<BufferItem>(10, funcTwo) { DegreeOfParallelism = 1 };
            pipeline.AddStep(stepTwo);

            //On Finish
            pipeline.OnFinishAsync += async (BufferItem input) =>
            {
                _logger.LogInformation($"Item {input.ItemId} processed");
                await Task.CompletedTask;
            };

            return pipeline;
        }
        private void PrintSummery()
        {
            _logger.LogInformation("-----------Summary-------------");
            _logger.LogInformation($"Total items enqueued: {ItemsSummery.GetNumberOfTotalItemsEnequed()}");
            _logger.LogInformation($"Total items successfully processed: {ItemsSummery.GetNumberOfItemsSuccessfullyProcessed()}");
            _logger.LogInformation($"Total items failed to process: {ItemsSummery.GetNumberOfItemsFailedToProcess()}");
        }
    }
}
