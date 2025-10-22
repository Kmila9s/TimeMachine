using TimeMachine.Models;
namespace TimeMachine.Repositories;

public class Timer : IDisposable
{
    private TimeClock _clock = new();
    public TimeSpan TimeElapsed { get; private set; }
    public double CurrentTick { get; private set; }
    public static event Action<bool>? OnPaused;
    public event Action<bool>? OnSimulationEnded;
    public event Action<double>? OnProgressAdvance;
    private int _lastPercent = 0;
    public volatile bool Paused = false;
    private CancellationTokenSource _cts = new(), _pauseCts = new();
    public Timer()
    {

    }
    public void Start(TimeSpan? Duration = null) => _ = Task.Run(() => StartSimulation(Duration), _cts.Token);
    private async Task StartSimulation(TimeSpan? Duration)
    {
        ulong maxTicks = 0;
        if (Duration != null)
        {
            maxTicks = GetSimulationTicks(Duration.Value, _clock.Ns);
        }
        while (!_cts.IsCancellationRequested)
        {
            if (!Paused)
            {
                CurrentTick = (ulong)_clock.Ns * _clock.Tick * (_clock.Multiplier > 0 ? _clock.Multiplier : 1);
                TimeElapsed = GetTimeFromTicks((ulong)CurrentTick, _clock.Ns);
                int percent = Convert.ToInt32(CurrentTick / maxTicks * 100);
                Console.WriteLine(percent);
                TotalPercent(percent);
                if (maxTicks != 0 && CurrentTick >= maxTicks)
                {
                    OnSimulationEnded?.Invoke(true);
                    _lastPercent = 0;
                    break;
                }
                _clock.Next();
                await Task.Delay(10);
            }
            else
                try
                {
                    await Task.Delay(Timeout.Infinite, _pauseCts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Expected when Continue() is called
                    Paused = false;
                    if (_cts.IsCancellationRequested)
                        break;
                }
            
        }
    }
    private void TotalPercent(int percent)
    {
        if(_lastPercent != percent)
        {
            _lastPercent = percent;
            OnProgressAdvance?.Invoke(percent);
        }
    }
    public void Restart(TimeSpan? Duration)
    {
        _cts.Cancel();
        _cts?.TryReset();
        _cts = new();
        Paused = false;
        _pauseCts?.Cancel();
        _pauseCts?.TryReset();
        _pauseCts = new();
        _clock = new();
        Paused = false;
        CurrentTick = 0;
        _ = Task.Run(() => StartSimulation(Duration), _cts.Token);
    }
    public void Stop()
    {
        _cts?.Cancel();
        Task.Delay(500).Wait();
        _pauseCts?.Cancel();
        _cts = new();
        _pauseCts = new();
        _clock = new();
        _lastPercent = 0;
        TimeElapsed = new();
        Paused = false;
        CurrentTick = 0;
    }
    public static ulong GetSimulationTicks(TimeSpan SimulationTime, int TickResolution)
    {
        double totalNs = SimulationTime.TotalSeconds * 1_000_000_000.0;
        return (ulong)Math.Round(totalNs / TickResolution);
    }
    public static TimeSpan GetTimeFromTicks(ulong simTicks, int tickResolutionNs = 10)
    {
        double totalSeconds = simTicks * (ulong)tickResolutionNs / 1_000_000_000.0;
        return TimeSpan.FromSeconds(totalSeconds);
    }
    public void Continue()
    {
        Paused = false;
        _pauseCts?.Cancel();
        _pauseCts?.TryReset();
        _pauseCts = new();
        OnPaused?.Invoke(Paused);
    }
    public void Pause()
    {
        Paused = true;
        OnPaused?.Invoke(Paused);
    }
    public void Dispose()
    {
        _pauseCts?.Cancel();
        _cts?.Cancel();
        _clock?.Dispose();  
        _pauseCts?.Dispose();
        _cts?.Dispose();

    }
}