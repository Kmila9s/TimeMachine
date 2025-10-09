using TimeMachine.Models;
namespace TimeMachine.Repositories;

public class Timer : IDisposable
{
    private TimeClock _clock = new();
    public TimeSpan TimeElapsed { get; private set; }
    public double CurrentTick { get; private set; }
    public static event Action<bool>? OnPaused, OnSimulationEnded;
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
                if (maxTicks != 0 && CurrentTick >= maxTicks)
                {
                    Pause();
                    OnSimulationEnded?.Invoke(true);
                }
                Console.WriteLine(TimeElapsed);
                _clock.Next();
                await Task.Yield();
            }
            else
                try
                {
                    await Task.Delay(Timeout.Infinite, _pauseCts.Token);
                }
                finally
                {
                    _pauseCts?.TryReset();
                }
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
        Paused = false;
        _ = Task.Run(() => StartSimulation(Duration), _cts.Token);
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