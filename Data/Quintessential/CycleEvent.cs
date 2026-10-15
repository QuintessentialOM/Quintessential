using System;
using static Quintessential.CycleEvent;

namespace Quintessential;

// TODO link to online documentation
/// <summary>
/// An event to be called each simulation cycle.<br/>
/// Register with <see cref="QApi.AddCycleEvent"/>.
/// </summary>
/// <param name="executionType">
/// The specific timing this event is meant to be invoked with.<br/>
/// Supports enum flags.
/// </param>
/// <param name="delegate">The delegate to execute.</param>
public class CycleEvent(CycleEventExecutionType executionType, CycleEventDelegate @delegate) {
    
    /// <summary>
    /// The specific timing this event is meant to be invoked with.<br/>
    /// Supports enum flags.
    /// </summary>
    public readonly CycleEventExecutionType ExecutionType = executionType;
    /// <summary>
    /// The delegate to execute.
    /// </summary>
    public readonly CycleEventDelegate Delegate = @delegate;

    /// <summary>
    /// Timing within a cycle. Supports flags! For a precise<br/>
    /// reference see <see cref="Sim.EndCycle"/> and <see cref="Sim.BeginCycle"/>.
    /// </summary>
    [Flags]
    public enum CycleEventExecutionType {
        /// <summary>
        /// The delegate will never be invoked.
        /// </summary>
        None = 0,
        /// <summary>
        /// The delegate will be invoked before all vanilla cycle code,<br/>
        /// but after the <see cref="PartSimState"/>s are cleared.
        /// </summary>
        First = 1,
        /// <summary>
        /// The delegate will be invoked before the first glyph phase,<br/>
        /// but after first instruction pass. E.g. Grab instructions.
        /// </summary>
        BeforeEarlyGlyphs = 2,
        /// <summary>
        /// The delegate will be invoked after the first glyph phase.
        /// </summary>
        AfterEarlyGlyphs = 4,
        /// <summary>
        /// The delegate will be invoked after all instructions.
        /// </summary>
        AfterInstructions = 8,
        /// <summary>
        /// The delegate will be invoked before the second glyph phase.
        /// </summary>
        BeforeLateGlyphs = 16,
        /// <summary>
        /// The delegate will be invoked after the second glyph phase.
        /// </summary>
        AfterLateGlyphs = 32,
        /// <summary>
        /// The delegate will be invoked just before the glyphs are reset.
        /// </summary>
        Last = 64,
    }
    /// <summary>
    /// The delegate that should contain the code to be invoked as a cycle event.
    /// </summary>
    /// <param name="sim">The current simulation.</param>
    /// <param name="executionType">The timing when the call was made. Useful if the <see cref="CycleEvent"/> is invoked on ones.</param>
    public delegate void CycleEventDelegate(patch_Sim sim, CycleEventExecutionType executionType);
}