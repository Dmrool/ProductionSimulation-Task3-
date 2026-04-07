using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionSimulation.Core
{
    public class ProductionLine
    {
        private readonly Machine _machine;
        private readonly Miller _miller;
        private readonly ILoader _loader;

        private CancellationTokenSource? _cts;
        private readonly Queue<Part> _buffer = new();
        private readonly SemaphoreSlim _millerLock = new(1, 1);

        public event EventHandler<string>? LogMessage;
        public event EventHandler<Part>? PartMoved;
        public event EventHandler<Part>? DefectFound;

        public ProductionLine(Machine machine, Miller miller, ILoader loader)
        {
            _machine = machine;
            _miller = miller;
            _loader = loader;

           
            _machine.PartProduced += OnMachineProduced;
            _miller.PartCompleted += OnMillerCompleted;
            _miller.DefectDetected += OnMillerDefect;
        }

        public async Task FeedMaterialAsync()
        {
            if (_cts?.IsCancellationRequested == true) return;

            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] 👤 Оператор подал заготовку в Станок {_machine.Id}...");

           
            await _machine.ProducePartAsync(_cts?.Token ?? CancellationToken.None);
        }

       
        private async void OnMachineProduced(object? sender, PartEventArgs e)
        {
            _buffer.Enqueue(e.Part);
            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] ✅ Станок {_machine.Id} изготовил деталь #{e.Part.Id}");
            await TryAssignToMillerAsync();
        }

        private async void OnMillerCompleted(object? sender, PartEventArgs e)
        {
            try { _millerLock.Release(); } catch { }

            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] Фрезеровщик {_miller.Id} -> Деталь #{e.Part.Id} готова");
            PartMoved?.Invoke(this, e.Part);

            await _loader.LoadAsync(e.Part, _cts?.Token ?? CancellationToken.None);
            PartMoved?.Invoke(this, e.Part);

            await TryAssignToMillerAsync();
        }

        private async void OnMillerDefect(object? sender, PartEventArgs e)
        {
            try { _millerLock.Release(); } catch { }

            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] ⚠️ БРАК! Фрезеровщик {_miller.Id} испортил деталь #{e.Part.Id}");
            DefectFound?.Invoke(this, e.Part);

            await Task.Delay(500, _cts?.Token ?? CancellationToken.None);
            PartMoved?.Invoke(this, e.Part);

            await TryAssignToMillerAsync();
        }

        private async Task TryAssignToMillerAsync()
        {
            if (await _millerLock.WaitAsync(0))
            {
                if (_buffer.TryDequeue(out var part))
                {
                    LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] Фрезеровщик {_miller.Id} -> Взял деталь #{part.Id}");
                    PartMoved?.Invoke(this, part);
                    _ = ProcessSafelyAsync(part);
                }
                else
                {
                    _millerLock.Release();
                }
            }
        }

        private async Task ProcessSafelyAsync(Part part)
        {
            try
            {
                await _miller.ProcessAsync(part, _cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка: {ex.Message}");
                try { _millerLock.Release(); } catch { }
            }
        }

        public void Start() => _cts = new CancellationTokenSource();
        public void Stop() => _cts?.Cancel();
    }
}