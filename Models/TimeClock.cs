namespace TimeMachine.Models;

public class TimeClock : IDisposable
{
    public ulong Tick { get; private set; } = 0;
    public ulong Multiplier { get; private set; } = 0;
    public int Ns { get; private set; } = 10;
    public static event Action<ulong>? OnTickChanged;
    public static event EventHandler<bool>? ResetCounter;
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
    public void Reset() => Dispose();
    public void Dispose()
    {
        Tick = 0;
    }
}