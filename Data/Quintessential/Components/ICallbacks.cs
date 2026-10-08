using static Quintessential.CycleEvent;

namespace Quintessential.Components;

public interface ISimCallbacks {
    /// <summary>
    /// Provides the values when <see cref="OnCycleCallback"/><br/>should be called for this object.
    /// </summary>
    public abstract CycleEventExecutionType CallbackType { get; }
    /// <summary>
    /// Called at specific times each cycle, if enabled at <see cref="CallbackType"/>.
    /// </summary>
    /// <param name="sim">The simulation running the cycle.</param>
    /// <param name="executionType">The timing of the execution.</param>
    public abstract void OnCycleCallback(patch_Sim sim, CycleEventExecutionType executionType);
}
