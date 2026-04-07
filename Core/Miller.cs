using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public class Miller
    {
        public string Id { get; }
        public int WorkMs { get; }

        
        public double DefectProbability { get; }

    
        public event PartHandler? PartCompleted;
        
        public event PartHandler? DefectDetected;

        public Miller(string id, int workMs, double defectProbability)
        {
            Id = id;
            WorkMs = workMs;
            DefectProbability = defectProbability;
        }

        public async Task ProcessAsync(Part part, CancellationToken ct)
        {
            part.State = PartState.AtOperator;

          
            await Task.Delay(WorkMs, ct);

            
            bool isDefect = new Random().NextDouble() < DefectProbability;

            if (isDefect)
            {
                part.IsDefective = true;
                
                DefectDetected?.Invoke(this, new PartEventArgs(part));
            }
            else
            {
                part.State = PartState.AtLoader;
              
                PartCompleted?.Invoke(this, new PartEventArgs(part));
            }
        }
    }
}