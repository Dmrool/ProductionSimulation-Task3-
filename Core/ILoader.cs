using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public interface ILoader
    {
        string Name { get; }
        Task LoadAsync(Part part, CancellationToken ct);
    }
}