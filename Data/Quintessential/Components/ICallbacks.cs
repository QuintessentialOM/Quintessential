using static Quintessential.CycleEvent;

namespace Quintessential.Components;

public interface ISimCallbacks {
    public abstract CycleEventExecutionType CallbackType { get; }
    public abstract void OnCycleCallback(patch_Sim sim, CycleEventExecutionType executionType);
}
