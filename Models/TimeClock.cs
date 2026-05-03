namespace TimeMachine.Models;

/// <summary>
/// Represents the global simulation clock. It provides the main "tick" that drives all time-based simulation events.
/// This class is responsible for advancing the simulation time incrementally.
/// </summary>
public class TimeClock : IDisposable
{
    /// <summary>
    /// Gets the current tick count since the last reset or overflow.
    /// </summary>
    public ulong Tick { get; private set; } = 0;
    /// <summary>
    /// Gets the number of times the Tick counter has overflowed. Used for very long simulations.
    /// </summary>
    public ulong Multiplier { get; private set; } = 0;
    /// <summary>
    /// Gets the tick resolution in nanoseconds. Defaults to 10 ns per tick.
    /// </summary>
    public int Ns { get; private set; } = 10;
    /// <summary>
    /// Defines the resolution of the simulation, representing how many nanoseconds pass in a single tick.
    /// </summary>
    public const int NS_PER_TICK = 10;
    /// <summary>
    /// Event fired on every single tick advancement. Provides the new tick count.
    /// </summary>
    public static event Action<ulong>? OnTickChanged;
    /// <summary>
    /// Event fired when the main tick counter overflows and resets to zero.
    /// </summary>
    public static event EventHandler<bool>? ResetCounter;
    /// <summary>
    /// Advances the simulation by one tick, firing <see cref="OnTickChanged"/>. Handles overflow by incrementing <see cref="Multiplier"/> and resetting.
    /// </summary>
    public void Next()
    {
        Tick++;
        if (Tick >= ulong.MaxValue - 1)
        {
            Multiplier++;
            ResetCounter?.Invoke(this, true);
            Tick = 0;
        }
        OnTickChanged?.Invoke(Tick);
    }

    /// <summary>
    /// Resets the tick counter to zero.
    /// </summary>
    public void Reset() => Dispose();
    /// <summary>
    /// Resets the tick counter to zero.
    /// </summary>
    public void Dispose()
    {
        Tick = 0;
    }
}