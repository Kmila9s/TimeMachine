namespace TimeMachine.Models;

public class Clock : IDisposable
{
    public string Name { get; set; } = string.Empty;
    public bool State { get; private set; } = false;
    public ulong Period { get; private set; }
    private readonly ulong _halfPeriod;
    private ulong _currentTick = 0;
    public event EventHandler<bool>? OnStatusChanged;
    public Clock(bool InitialState, double frequencyMHz, string Name = "")
    {
        this.Name = Name;
        State = InitialState;
        double periodNs = 1_000_000_000.0 / frequencyMHz;
        Period = (ulong)(periodNs / 10);
        _halfPeriod = Period / 2;
        TimeClock.OnTickChanged += Tick;
        TimeClock.ResetCounter += OnResetGlobalClock;
    }
    private void OnResetGlobalClock(object? sender, bool reset)
    {
        if (reset)
        {
            _currentTick = 0;
        }
    }
    private void Tick(ulong globalTick)
    {
        if (globalTick >= _currentTick)
        {
            State = !State;
            OnStatusChanged?.Invoke(this, State);
            _currentTick += _halfPeriod;
        }
    }
    public void Dispose()
    {
        TimeClock.OnTickChanged -= Tick;
        TimeClock.ResetCounter -= OnResetGlobalClock;
    }
}