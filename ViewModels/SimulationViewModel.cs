using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ProductionSimulation.Core;
using ProductionSimulation.Services;

namespace ProductionSimulation.ViewModels
{
    public class SimulationViewModel : BaseViewModel
    {
        private readonly ProductionLine _line;
        private readonly DispatcherTimer _moveTimer;
        private readonly Dispatcher _dispatcher;
        private double _targetX;
        private double _currentX;

        public string Title { get; }
        public ObservableCollection<string> Log { get; } = new();

        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ProducePartCommand { get; } 

        public bool IsRunning { get; private set; }

       
        private bool _isMachineBusy;
        public bool IsMachineBusy
        {
            get => _isMachineBusy;
            private set => SetProperty(ref _isMachineBusy, value);
        }

        public double PartX
        {
            get => _currentX;
            private set => SetProperty(ref _currentX, value);
        }

        private string _partColor = "#2196F3";
        public string PartColor
        {
            get => _partColor;
            private set => SetProperty(ref _partColor, value);
        }

        public SimulationViewModel(ProductionLine line, string title)
        {
            _line = line;
            Title = title;
            _dispatcher = Dispatcher.CurrentDispatcher;

            _moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _moveTimer.Tick += (s, e) => SmoothMove();

            StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsRunning);

            
            ProducePartCommand = new RelayCommand(_ => ProducePart(), _ => IsRunning && !IsMachineBusy);

            _line.LogMessage += (s, msg) => _dispatcher.Invoke(() => Log.Add(msg));

            _line.PartMoved += (s, part) => _dispatcher.Invoke(() =>
            {
                if (part.State == PartState.Delivered) PartColor = "#2196F3";
                Log.Add(StateInspector.Inspect(part));
                UpdateTarget(part.State);
            });

            _line.DefectFound += (s, part) => _dispatcher.Invoke(() =>
            {
                PartColor = "#F44336";
                Log.Add(StateInspector.Inspect(part));
                _targetX = 420;
                if (!_moveTimer.IsEnabled) _moveTimer.Start();
            });
        }

        
        private async void ProducePart()
        {
            if (!IsRunning) return;

            IsMachineBusy = true; 
            ProducePartCommand.RaiseCanExecuteChanged();

            try
            {
              
                await _line.FeedMaterialAsync();
            }
            finally
            {
                IsMachineBusy = false; 
                ProducePartCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateTarget(PartState state)
        {
            _targetX = state switch
            {
                PartState.AtMachine => 60,
                PartState.AtOperator => 180,
                PartState.AtLoader => 300,
                PartState.Delivered => 420,
                _ => _currentX
            };
            if (!_moveTimer.IsEnabled) _moveTimer.Start();
        }

        private void SmoothMove()
        {
            if (Math.Abs(_currentX - _targetX) < 0.5)
            {
                PartX = _targetX;
                _moveTimer.Stop();
                return;
            }
            PartX += (_targetX - _currentX) * 0.15;
        }

        private void Start()
        {
            IsRunning = true;
            _line.Start();
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            ProducePartCommand.RaiseCanExecuteChanged();
        }

        private void Stop()
        {
            IsRunning = false;
            _line.Stop();
            _moveTimer.Stop();
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            ProducePartCommand.RaiseCanExecuteChanged();
        }
    }
}