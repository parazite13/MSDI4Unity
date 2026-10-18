using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace MSDI4Unity.Host
{
    public class UnityApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource startedCts = new();
        private readonly CancellationTokenSource stoppingCts = new();
        private readonly CancellationTokenSource stoppedCts = new();

        public CancellationToken ApplicationStarted => startedCts.Token;
        public CancellationToken ApplicationStopping => stoppingCts.Token;
        public CancellationToken ApplicationStopped => stoppedCts.Token;

        public UnityApplicationLifetime()
        {
            // Mark application as started
            startedCts.Cancel();
        }

        public void StopApplication()
        {
            lock (stoppingCts)
            {
                if (!stoppingCts.IsCancellationRequested)
                {
                    stoppingCts.Cancel();
                    stoppedCts.Cancel();
                }
            }
        }

        public void Dispose()
        {
            startedCts.Dispose();
            stoppingCts.Dispose();
            stoppedCts.Dispose();
        }
    }
}