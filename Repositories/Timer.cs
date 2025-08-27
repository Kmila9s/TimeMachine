using TimeMachine.Models;
namespace TimeMachine.Repositories;

public class Timer : IDisposable
{
    public TimeClock _clock = new();
    public TimeSpan TimeElapsed { get; private set; }
    public static event Action<bool>? OnPaused;
    public volatile bool Paused = false;
    private readonly CancellationTokenSource _cts = new();
    private CancellationTokenSource _pauseCts = new();
    public Timer()
    {
        _ = Task.Run(Start, _cts.Token);
    }
    private async Task Start()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (!Paused)
            {
                double totalNs = (ulong)_clock.Ns * _clock.Tick * (_clock.Multiplier > 0 ? _clock.Multiplier : 1);
                double totalTicks = totalNs / 100.0;
                TimeElapsed = TimeSpan.FromTicks((long)totalTicks);
                _clock.Next();
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