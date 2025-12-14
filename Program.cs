using TimeMachine.Models;

namespace TimeMachine;

/// <summary>
/// Console test application for the TimeMachine module.
/// </summary>
/// <remarks>
/// <para>
/// This program demonstrates the TimeMachine module's capabilities by creating
/// a simulation timer with two clock signals of different frequencies and
/// providing an interactive command-line interface for controlling the simulation.
/// </para>
/// <para>
/// <b>Test Configuration:</b>
/// <list type="bullet">
/// <item><description>Timer runs indefinitely (no fixed duration)</description></item>
/// <item><description>First clock: 50 MHz, starts high</description></item>
/// <item><description>Second clock: 100 MHz, starts low</description></item>
/// </list>
/// </para>
/// </remarks>
class Program
{
    /// <summary>
    /// Entry point for the TimeMachine test console application.
    /// </summary>
    /// <param name="args">Command-line arguments (currently unused).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// <b>Available Commands:</b>
    /// <list type="table">
    /// <listheader>
    /// <term>Command</term>
    /// <description>Action</description>
    /// </listheader>
    /// <item>
    /// <term>play</term>
    /// <description>Resumes the simulation if paused</description>
    /// </item>
    /// <item>
    /// <term>stop</term>
    /// <description>Pauses the running simulation</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The program creates two <see cref="Clock"/> instances to demonstrate
    /// multiple clock domain handling:
    /// <list type="bullet">
    /// <item><description><c>firstClock</c>: 50 MHz clock starting in high state</description></item>
    /// <item><description><c>secondClock</c>: 100 MHz clock starting in low state</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Note:</b> The application runs in an infinite loop and must be terminated
    /// externally (Ctrl+C or closing the console window).
    /// </para>
    /// </remarks>
    static async Task Main(string[] args)
    {
        // Create timer with indefinite duration (null = no fixed duration)
        Repositories.Timer timer = new();
        timer.Start(null);

        // Create two clock signals with different frequencies
        // Clock constructor: Clock(bool initialState, int frequencyMHz)
        Clock firstClock = new(true, 50);    // 50 MHz clock, starts high
        Clock secondClock = new(false, 100); // 100 MHz clock, starts low

        // Interactive command loop
        while (true)
        {
            string? input = Console.ReadLine();
            if (!String.IsNullOrEmpty(input))
            {
                if (input == "play")
                {
                    timer.Continue();
                }
                else if (input == "stop")
                {
                    timer.Pause();
                }
            }
        }
    }
}
