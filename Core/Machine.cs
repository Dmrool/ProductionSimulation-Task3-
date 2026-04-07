using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public class PartEventArgs : EventArgs
    {
        public Part Part { get; }
        public PartEventArgs(Part part) => Part = part;
    }

    public delegate void PartHandler(object sender, PartEventArgs e);
    public delegate void StatusHandler(object sender, string message);

    public class Machine
    {
        public string Id { get; }
        public int CycleMs { get; } 

       
        public event PartHandler? PartProduced;

      
        private static int _globalCounter = 0;

        public Machine(string id, int cycleMs)
        {
            Id = id;
            CycleMs = cycleMs;
        }

        
        public async Task ProducePartAsync(CancellationToken ct)
        {
            await Task.Delay(CycleMs, ct);

            if (ct.IsCancellationRequested) return;

            int partId = Interlocked.Increment(ref _globalCounter);
            var part = new Part(partId);

            PartProduced?.Invoke(this, new PartEventArgs(part));
        }
    }
}