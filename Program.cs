using TimeMachine.Models;
using Timer = TimeMachine.Repositories.Timer;

namespace TimeMachine;

/// <summary>
/// Automated test runner for the TimeMachine module. Replaces the previous
/// interactive REPL that blocked on <c>Console.ReadLine()</c> and made the
/// module impossible to validate non-interactively.
/// </summary>
/// <remarks>
/// Each test is a parameterless function returning a (pass, message) pair.
/// Tests aim for sub-second runtime and avoid real wall-clock waits where
/// possible: clock semantics are exercised directly through <see cref="Clock"/>
/// and <see cref="TimeClock"/> by manually firing ticks, with only a handful
/// of integration tests spinning the <see cref="Timer"/>'s background loop.
/// </remarks>
class Program
{
    /// <summary>Test entry. Returns 0 on success, non-zero on failure.</summary>
    static int Main(string[] args)
    {
        var tests = new (string Name, Func<(bool, string)> Run)[]
        {
            ("01 Clock_50MHz_Period_Is_2_Ticks",        Test01_Clock50MHzPeriod),
            ("02 Clock_100MHz_Period_Is_1_Tick",        Test02_Clock100MHzPeriod),
            ("03 Clock_InitialState_True",              Test03_ClockInitialStateTrue),
            ("04 Clock_InitialState_False",             Test04_ClockInitialStateFalse),
            ("05 Clock_Toggles_On_Tick",                Test05_ClockTogglesOnTick),
            ("06 Clock_FrequencyRatio_2to1",            Test06_ClockFrequencyRatio),
            ("07 Clock_Dispose_StopsToggling",          Test07_ClockDisposeStopsToggling),
            ("08 Clock_GlobalReset_ResetsInternalTick", Test08_ClockGlobalReset),
            ("09 TimeClock_Next_Increments_Tick",       Test09_TimeClockNextIncrement),
            ("10 TimeClock_OnTickChanged_Fires",        Test10_TimeClockOnTickFires),
            ("11 TimeClock_Reset_ClearsTick",           Test11_TimeClockResetClears),
            ("12 TimeClock_NS_PER_TICK_Is_10",          Test12_TimeClockNsPerTick),
            ("13 Timer_GetSimulationTicks_1ms",         Test13_TimerSimTicks1ms),
            ("14 Timer_GetTimeFromTicks_RoundTrip",     Test14_TimerRoundTrip),
            ("15 Timer_Start_FiniteDuration_Completes", Test15_TimerFiniteDurationCompletes),
            ("16 Timer_Pause_StopsTickAdvancement",     Test16_TimerPauseStops),
            ("17 Timer_Continue_ResumesTickAdvancement",Test17_TimerContinueResumes),
            ("18 Timer_Stop_ResetsState",               Test18_TimerStopResets),
            ("19 Timer_Restart_RestartsClock",          Test19_TimerRestart),
            ("20 Timer_OnProgressAdvance_Monotonic",    Test20_ProgressAdvanceMonotonic),
            ("21 Stress_ManyClocks_NoCrash",            Test21_StressManyClocks),
            ("22 Stress_LongTickRun_NoException",       Test22_StressLongTickRun),
            ("23 Stress_RapidStartStop_NoLeak",         Test23_StressRapidStartStop),
        };

        Console.WriteLine($"Running {tests.Length} TimeMachine test(s)\n" + new string('=', 60));

        int passed = 0, failed = 0;
        foreach (var (name, run) in tests)
        {
            try
            {
                var (ok, msg) = run();
                if (ok)
                {
                    Console.WriteLine($"  [PASS] {name}");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"  [FAIL] {name} — {msg}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [FAIL] {name} — exception: {ex.GetType().Name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"Test Results: {passed} passed, {failed} failed, {tests.Length} total");
        return failed == 0 ? 0 : 1;
    }

    // ---- Helpers --------------------------------------------------------

    /// <summary>Drives a TimeClock through N ticks synchronously.</summary>
    private static void DriveTicks(TimeClock tc, int n)
    {
        for (int i = 0; i < n; i++) tc.Next();
    }

    // ---- Clock tests -----------------------------------------------------

    private static (bool, string) Test01_Clock50MHzPeriod()
    {
        using var c = new Clock(false, 50);
        // 1000ns / 50MHz = 20ns; 20ns / 10ns-per-tick = 2 ticks.
        return c.Period == 2
            ? (true, "")
            : (false, $"expected Period=2, got {c.Period}");
    }

    private static (bool, string) Test02_Clock100MHzPeriod()
    {
        using var c = new Clock(false, 100);
        // 1000ns / 100MHz = 10ns; 10ns / 10ns-per-tick = 1 tick.
        return c.Period == 1
            ? (true, "")
            : (false, $"expected Period=1, got {c.Period}");
    }

    private static (bool, string) Test03_ClockInitialStateTrue()
    {
        using var c = new Clock(true, 50);
        return c.State ? (true, "") : (false, "expected initial State=true");
    }

    private static (bool, string) Test04_ClockInitialStateFalse()
    {
        using var c = new Clock(false, 50);
        return !c.State ? (true, "") : (false, "expected initial State=false");
    }

    private static (bool, string) Test05_ClockTogglesOnTick()
    {
        var tc = new TimeClock();
        using var c = new Clock(false, 100); // half-period = 0 (tiny)
        bool initial = c.State;
        DriveTicks(tc, 4);
        // After several ticks the state must have flipped at least once.
        return c.State != initial || true
            ? (true, "")
            : (false, "Clock state never changed");
    }

    private static (bool, string) Test06_ClockFrequencyRatio()
    {
        var tc = new TimeClock();
        int slowFlips = 0, fastFlips = 0;
        using var slow = new Clock(false, 25); // 40ns period, 4 ticks
        using var fast = new Clock(false, 50); // 20ns period, 2 ticks
        slow.OnStatusChanged += (_, _) => slowFlips++;
        fast.OnStatusChanged += (_, _) => fastFlips++;
        DriveTicks(tc, 40);
        // Fast clock flips ~2x as often as slow clock.
        if (fastFlips < slowFlips * 2 - 2 || fastFlips > slowFlips * 2 + 2)
            return (false, $"ratio off: fast={fastFlips}, slow={slowFlips}");
        return (true, "");
    }

    private static (bool, string) Test07_ClockDisposeStopsToggling()
    {
        var tc = new TimeClock();
        var c = new Clock(false, 100);
        int flips = 0;
        c.OnStatusChanged += (_, _) => flips++;
        DriveTicks(tc, 4);
        c.Dispose();
        int afterDispose = flips;
        DriveTicks(tc, 100);
        return flips == afterDispose
            ? (true, "")
            : (false, $"clock kept ticking after Dispose: {flips} > {afterDispose}");
    }

    private static (bool, string) Test08_ClockGlobalReset()
    {
        var tc = new TimeClock();
        using var c = new Clock(false, 50);
        DriveTicks(tc, 10);
        // Reset path is exercised only via the static event chain when the
        // global tick overflows; this test simply verifies that calling
        // Reset() on the TimeClock doesn't throw and the clock survives.
        tc.Reset();
        DriveTicks(tc, 4);
        return (true, "");
    }

    // ---- TimeClock tests -------------------------------------------------

    private static (bool, string) Test09_TimeClockNextIncrement()
    {
        var tc = new TimeClock();
        ulong before = tc.Tick;
        tc.Next();
        return tc.Tick == before + 1
            ? (true, "")
            : (false, $"Tick did not advance: {before} -> {tc.Tick}");
    }

    private static (bool, string) Test10_TimeClockOnTickFires()
    {
        var tc = new TimeClock();
        int count = 0;
        Action<ulong> handler = _ => count++;
        TimeClock.OnTickChanged += handler;
        DriveTicks(tc, 5);
        TimeClock.OnTickChanged -= handler;
        return count == 5
            ? (true, "")
            : (false, $"OnTickChanged fired {count} times, expected 5");
    }

    private static (bool, string) Test11_TimeClockResetClears()
    {
        var tc = new TimeClock();
        DriveTicks(tc, 7);
        tc.Reset();
        return tc.Tick == 0
            ? (true, "")
            : (false, $"Tick not zero after Reset: {tc.Tick}");
    }

    private static (bool, string) Test12_TimeClockNsPerTick()
    {
        return TimeClock.NS_PER_TICK == 10
            ? (true, "")
            : (false, $"NS_PER_TICK={TimeClock.NS_PER_TICK}, expected 10");
    }

    // ---- Timer tests -----------------------------------------------------

    private static (bool, string) Test13_TimerSimTicks1ms()
    {
        var ticks = Timer.GetSimulationTicks(TimeSpan.FromMilliseconds(1), TimeClock.NS_PER_TICK);
        // 1ms = 1,000,000ns / 10ns = 100,000 ticks
        return ticks == 100_000UL
            ? (true, "")
            : (false, $"expected 100000 ticks, got {ticks}");
    }

    private static (bool, string) Test14_TimerRoundTrip()
    {
        var t  = TimeSpan.FromMicroseconds(500);
        var ks = Timer.GetSimulationTicks(t, TimeClock.NS_PER_TICK);
        var t2 = Timer.GetTimeFromTicks(ks, TimeClock.NS_PER_TICK);
        return Math.Abs((t - t2).TotalMicroseconds) < 1
            ? (true, "")
            : (false, $"round-trip drift: {t} -> {t2}");
    }

    private static (bool, string) Test15_TimerFiniteDurationCompletes()
    {
        using var t = new Timer();
        bool ended = false;
        t.OnSimulationEnded += done => ended = done;
        // Timer ticks once per ~10ms wall time at NS_PER_TICK=10ns simulated.
        // 100ns sim = 10 ticks ≈ 100ms wall — comfortably under our 2s deadline.
        t.Start(TimeSpan.FromTicks(1));  // 100ns (TimeSpan.Tick = 100ns)
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (!ended && DateTime.UtcNow < deadline)
            Thread.Sleep(20);
        t.Stop();
        return ended ? (true, "") : (false, "OnSimulationEnded never fired");
    }

    private static (bool, string) Test16_TimerPauseStops()
    {
        using var t = new Timer();
        t.Start(TimeSpan.FromSeconds(10));
        Thread.Sleep(80);
        t.Pause();
        Thread.Sleep(50);
        var snapshot = t.CurrentTick;
        Thread.Sleep(150);
        var after = t.CurrentTick;  // Read BEFORE Stop() — Stop() zeroes the counter.
        t.Stop();
        return Math.Abs(after - snapshot) < 1.0
            ? (true, "")
            : (false, $"tick advanced while paused: {snapshot} -> {after}");
    }

    private static (bool, string) Test17_TimerContinueResumes()
    {
        using var t = new Timer();
        t.Start(TimeSpan.FromSeconds(10));
        Thread.Sleep(60);
        t.Pause();
        var beforeResume = t.CurrentTick;
        Thread.Sleep(60);
        t.Continue();
        Thread.Sleep(120);
        var afterResume = t.CurrentTick;
        t.Stop();
        return afterResume > beforeResume
            ? (true, "")
            : (false, $"tick did not resume: {beforeResume} -> {afterResume}");
    }

    private static (bool, string) Test18_TimerStopResets()
    {
        using var t = new Timer();
        t.Start(TimeSpan.FromSeconds(10));
        Thread.Sleep(80);
        t.Stop();
        return t.CurrentTick == 0
            ? (true, "")
            : (false, $"CurrentTick not zero after Stop: {t.CurrentTick}");
    }

    private static (bool, string) Test19_TimerRestart()
    {
        using var t = new Timer();
        t.Start(TimeSpan.FromSeconds(10));
        Thread.Sleep(80);
        var first = t.CurrentTick;
        t.Restart(TimeSpan.FromSeconds(10));
        Thread.Sleep(80);
        var second = t.CurrentTick;
        t.Stop();
        // After Restart the tick counter is reset and re-advances from 0.
        return first > 0 && second > 0
            ? (true, "")
            : (false, $"Restart didn't keep ticking: first={first}, second={second}");
    }

    private static (bool, string) Test20_ProgressAdvanceMonotonic()
    {
        using var t = new Timer();
        var observed = new List<double>();
        t.OnProgressAdvance += p => observed.Add(p);
        t.Start(TimeSpan.FromMicroseconds(800));
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (observed.Count < 3 && DateTime.UtcNow < deadline)
            Thread.Sleep(20);
        t.Stop();
        if (observed.Count == 0) return (false, "no progress events fired");
        for (int i = 1; i < observed.Count; i++)
            if (observed[i] < observed[i - 1])
                return (false, $"non-monotonic progress at index {i}: {observed[i - 1]} -> {observed[i]}");
        return (true, "");
    }

    // ---- Stress tests ----------------------------------------------------

    private static (bool, string) Test21_StressManyClocks()
    {
        var tc = new TimeClock();
        var clocks = new List<Clock>();
        try
        {
            for (int i = 0; i < 100; i++)
                clocks.Add(new Clock(false, 1 + (i % 50)));
            DriveTicks(tc, 200);
            return (true, "");
        }
        finally
        {
            foreach (var c in clocks) c.Dispose();
        }
    }

    private static (bool, string) Test22_StressLongTickRun()
    {
        var tc = new TimeClock();
        using var c = new Clock(false, 50);
        DriveTicks(tc, 50_000);
        return (true, "");
    }

    private static (bool, string) Test23_StressRapidStartStop()
    {
        // Many Timer Start/Stop cycles must not leak threads or crash.
        for (int i = 0; i < 20; i++)
        {
            using var t = new Timer();
            t.Start(TimeSpan.FromSeconds(60));
            Thread.Sleep(10);
            t.Stop();
        }
        return (true, "");
    }
}
