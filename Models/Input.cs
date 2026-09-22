namespace TimeMachine.Models;

/// <summary>
/// Represents an input signal or variable in the VHDL simulation.
/// </summary>
/// <remarks>
/// <para>
/// The Input class is a placeholder for future implementation of simulation input handling.
/// It will be used to define input signals that can be controlled during simulation,
/// allowing users to set input values at specific time points.
/// </para>
/// <para>
/// <b>Future Use Cases:</b>
/// <list type="bullet">
/// <item><description>Defining testbench inputs for entity ports</description></item>
/// <item><description>Scheduling input value changes at specific simulation times</description></item>
/// <item><description>Providing user-controllable stimulus for interactive debugging</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Current Status:</b> Placeholder implementation. The class currently only holds
/// the input name. Future versions will add properties for value, type, and time-based
/// value changes.
/// </para>
/// </remarks>
/// <seealso cref="Clock"/>
/// <seealso cref="TimeClock"/>
public class Input
{
    /// <summary>
    /// Gets or sets the name of the input signal.
    /// </summary>
    /// <value>
    /// The identifier name for this input signal.
    /// Defaults to an empty string.
    /// </value>
    /// <example>
    /// <code>
    /// var resetInput = new Input { Name = "reset" };
    /// var dataInput = new Input { Name = "data_in" };
    /// </code>
    /// </example>
    public string Name { get; set; } = string.Empty;
}