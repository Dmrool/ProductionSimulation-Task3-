using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public class ConveyorLoader : ILoader
    {
        public string Name { get; } = "Конвейерный погрузчик";
        private readonly int _speedMs;

        public ConveyorLoader(int speedMs) => _speedMs = speedMs;

        public async Task LoadAsync(Part part, CancellationToken ct)
        {
          
            await Task.Delay(_speedMs, ct);
            part.State = PartState.Delivered;
        }
    }
}