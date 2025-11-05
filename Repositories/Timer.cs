using TimeMachine.Models;
namespace TimeMachine.Repositories;

/// <summary>
/// Manages the discrete time simulation, providing controls to start, pause, continue, and stop.
/// This class is the core of the time simulation engine, running on a background thread.
/// </summary>
public class Timer : IDisposable
{
    private TimeClock _clock = new();

    /// <summary>
    /// Gets the total simulated time that has elapsed since the simulation started.
    /// </summary>
    public TimeSpan TimeElapsed { get; private set; }

    /// <summary>
    /// Gets the current simulation tick count, representing the total elapsed time in nanoseconds.
    /// </summary>
    public double CurrentTick { get; private set; }

    /// <summary>
    /// Event fired when the simulation is paused or resumed. The boolean payload is true if paused, false if resumed.
    /// </summary>
    public static event Action<bool>? OnPaused;

    /// <summary>
    /// Event fired when a simulation with a fixed duration has completed. The boolean payload is true upon completion.
    /// </summary>
    public event Action<bool>? OnSimulationEnded;

    /// <summary>
    /// Event fired when the simulation progress percentage increases. The double payload is the new percentage.
    /// </summary>
    public event Action<double>? OnProgressAdvance;
    private int _lastPercent = 0;

    /// <summary>
    /// Indicates whether the simulation is currently paused.
    /// </summary>
    public volatile bool Paused = false;
    private CancellationTokenSource _cts = new(), _pauseCts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class.
    /// </summary>
    public Timer()
    {

    }

    /// <summary>
    /// Starts the simulation on a background thread.
    /// </summary>
    /// <param name="Duration">An optional fixed duration for the simulation. If null, the simulation runs indefinitely.</param>
    public void Start(TimeSpan? Duration = null) => _ = Task.Run(() => StartSimulation(Duration), _cts.Token);

    /// <summary>
    /// The main simulation loop that advances time and fires events.
    /// </summary>
    /// <param name="Duration">The total duration the simulation should run for. Used to calculate progress.</param>
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
                if (maxTicks > 0)
                {
                    int percent = Convert.ToInt32(CurrentTick / maxTicks * 100);
                    TotalPercent(percent);
                }
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

    /// <summary>
    /// Invokes the OnProgressAdvance event only when the percentage value has changed.
    /// </summary>
    /// <param name="percent">The current progress percentage.</param>
    private void TotalPercent(int percent)
    {
        if(_lastPercent != percent)
        {
            _lastPercent = percent;
            OnProgressAdvance?.Invoke(percent);
        }
    }

    /// <summary>
    /// Stops the current simulation and immediately starts a new one.
    /// </summary>
    /// <param name="Duration">An optional fixed duration for the new simulation.</param>
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

    /// <summary>
    /// Stops the simulation completely and resets all state.
    /// </summary>
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

    /// <summary>
    /// Calculates the total number of simulation ticks required to represent a given TimeSpan.
    /// </summary>
    /// <param name="SimulationTime">The duration to convert to ticks.</param>
    /// <param name="TickResolution">The resolution of a single tick in nanoseconds.</param>
    /// <returns>The total number of ticks.</returns>
    public static ulong GetSimulationTicks(TimeSpan SimulationTime, int TickResolution)
    {
        double totalNs = SimulationTime.TotalSeconds * 1_000_000_000.0;
        return (ulong)Math.Round(totalNs / TickResolution);
    }

    /// <summary>
    /// Converts a number of simulation ticks back into a TimeSpan.
    /// </summary>
    /// <param name="simTicks">The number of simulation ticks.</param>
    /// <param name="tickResolutionNs">The resolution of a single tick in nanoseconds.</param>
    /// <returns>The corresponding TimeSpan duration.</returns>
    public static TimeSpan GetTimeFromTicks(ulong simTicks, int tickResolutionNs = 10)
    {
        double totalSeconds = simTicks * (ulong)tickResolutionNs / 1_000_000_000.0;
        return TimeSpan.FromSeconds(totalSeconds);
    }

    /// <summary>
    /// Resumes a paused simulation.
    /// </summary>
    public void Continue()
    {
        Paused = false;
        _pauseCts?.Cancel();
        _pauseCts?.TryReset();
        _pauseCts = new();
        OnPaused?.Invoke(Paused);
    }

    /// <summary>
    /// Pauses the currently running simulation.
    /// </summary>
    public void Pause()
    {
        Paused = true;
        OnPaused?.Invoke(Paused);
    }

    /// <summary>
    /// Disposes of managed resources, such as cancellation tokens and the internal clock.
    /// </summary>
    public void Dispose()
    {
        _pauseCts?.Cancel();
        _cts?.Cancel();
        _clock?.Dispose();  
        _pauseCts?.Dispose();
        _cts?.Dispose();

    }
}