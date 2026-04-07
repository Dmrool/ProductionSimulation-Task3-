using System.Collections.ObjectModel;
using System.Windows.Input;
using ProductionSimulation.Core;

namespace ProductionSimulation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<SimulationViewModel> Simulations { get; } = new();
        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }

        public MainViewModel()
        {
            AddCommand = new RelayCommand(_ => AddLine());
            RemoveCommand = new RelayCommand(_ => RemoveLine(), _ => Simulations.Count > 0);
        }

        private void AddLine()
        {
            int id = Simulations.Count + 1;
            var machine = new Machine($"Станок-{id}", 1500 + id * 100);
            var miller = new Miller($"Фрезеровщик-{id}", 2500 + id * 200, 0.2);
            var loader = new ConveyorLoader(1000);

            var line = new ProductionLine(machine, miller, loader);

            Simulations.Add(new SimulationViewModel(line, $"Линия #{id} (Брак ~20%)"));
            RemoveCommand.RaiseCanExecuteChanged();
        }

        private void RemoveLine()
        {
            if (Simulations.Count == 0) return;
            var last = Simulations[^1];
            last.StopCommand.Execute(null);
            Simulations.RemoveAt(Simulations.Count - 1);
            RemoveCommand.RaiseCanExecuteChanged();
        }
    }
}