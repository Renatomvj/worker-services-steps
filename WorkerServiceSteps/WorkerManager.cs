using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public class WorkerManager : IWorkerManager
    {
        private readonly IHostedService _hostedService;
        public WorkerManager(IHostedService hostedService)
        {
            _hostedService = hostedService;
        }
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            Task.Factory.StartNew(async () =>
            {
                var character = Console.ReadKey();               

                await _hostedService.StopAsync(new CancellationToken(true));
            }
           );
            
        }
    }
}
