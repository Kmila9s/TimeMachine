namespace TimeMachine.Models;

/// <summary>
/// Represents a single clock signal within the simulation. It toggles its state based on its frequency
/// and the global simulation ticks provided by <see cref="TimeClock"/>.
/// </summary>
public class Clock : IDisposable
{
    /// <summary>
    /// Gets or sets the name of the clock signal.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets the current state of the clock (e.g., high or low).
    /// </summary>
    public bool State { get; private set; } = false;
    /// <summary>
    /// Gets the period of the clock in simulation ticks.
    /// </summary>
    public ulong Period { get; private set; }
    /// <summary>
    /// Half of the period in simulation ticks. Used to determine when the clock state should toggle.
    /// </summary>
    private readonly ulong _halfPeriod;
    /// <summary>
    /// Tracks the next tick at which the clock should toggle its state.
    /// </summary>
    private ulong _currentTick = 0;
    /// <summary>
    /// Event fired whenever the clock's state changes.
    /// </summary>
    public event EventHandler<bool>? OnStatusChanged;
    /// <summary>
    /// Initializes a new instance of the <see cref="Clock"/> class.
    /// </summary>
    /// <param name="InitialState">The initial state of the clock (true for high, false for low).</param>
    /// <param name="frequencyMHz">The frequency of the clock in Megahertz.</param>
    /// <param name="Name">An optional name for the clock signal.</param>
    public Clock(bool InitialState, double frequencyMHz, string Name = "")
    {
        this.Name = Name;
        State = InitialState;
        double periodNs = 1_000.0 / frequencyMHz; // 1_000,000,000 / 1,000,000
        Period = (ulong)(periodNs / TimeClock.NS_PER_TICK);
        _halfPeriod = Period / 2;
        TimeClock.OnTickChanged += Tick;
        TimeClock.ResetCounter += OnResetGlobalClock;
    }
    /// <summary>
    /// Handles the global clock overflow reset by resetting the internal tick counter to zero.
    /// </summary>
    /// <param name="sender">The source of the event (the <see cref="TimeClock"/> instance).</param>
    /// <param name="reset">True if the global clock has been reset due to overflow.</param>
    private void OnResetGlobalClock(object? sender, bool reset)
    {
        if (reset)
        {
            _currentTick = 0;
        }
    }
    /// <summary>
    /// Callback method that is invoked on every global tick to determine if this clock's state should change.
    /// </summary>
    /// <param name="globalTick">The current global simulation tick.</param>
    private void Tick(ulong globalTick)
    {
        if (globalTick >= _currentTick)
        {
            State = !State;
            OnStatusChanged?.Invoke(this, State);
            _currentTick += _halfPeriod;
        }
    }

    /// <summary>
    /// Unsubscribes from global clock events to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        TimeClock.OnTickChanged -= Tick;
        TimeClock.ResetCounter -= OnResetGlobalClock;
    }
}