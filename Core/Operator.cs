using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public class Operator
    {
        public string Id { get; }
        public int WorkMs { get; }
        public event PartHandler? PartCompleted;

        public Operator(string id, int workMs)
        {
            Id = id;
            WorkMs = workMs;
        }

        public async Task ProcessAsync(Part part, CancellationToken ct)
        {
            part.State = PartState.AtOperator;
            await Task.Delay(WorkMs, ct);
            part.State = PartState.AtLoader;
            PartCompleted?.Invoke(this, new PartEventArgs(part));
        }
    }
}