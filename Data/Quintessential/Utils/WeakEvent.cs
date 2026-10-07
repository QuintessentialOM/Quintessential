using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

//namespace Quintessential.Utils;
/*
//# Currently unused, see https://ladimolnar.com/2015/09/14/the-weak-event-pattern-is-dangerous/
//# for a prominent issue.
//# Kept here since even with that there are applications for such a system.
public abstract class WeakEventBase<TAction> where TAction : class {
    protected ImmutableList<WeakReference<TAction>> _listeners = [];
    //~ Dictionary, but with liked Garbage collection.
    //~ Keep the delegates alive with their target. This prevents anonymous delegates from being garbage collected prematurely.
    private readonly ConditionalWeakTable<object, List<object>> _delegateKeepAlive = [];
    public void operator +=(TAction handler) {
        if (handler == null)
            return;

        var weakReference = new WeakReference<TAction>(handler);
        _listeners = _listeners.Add(weakReference);
        if (GetTarget(handler) != null) {
            _delegateKeepAlive.GetOrCreateValue(GetTarget(handler)).Add(handler);
        }
    }
    public void operator -=(TAction handler) {
        if (handler == null)
            return;

        // Remove the handler and all handlers that have been garbage collected
        _listeners = _listeners.RemoveAll(wr => !wr.TryGetTarget(out var target) || handler.Equals(target));
        if (GetTarget(handler) != null && _delegateKeepAlive.TryGetValue(GetTarget(handler), out var weakReference)) {
            weakReference.Remove(handler);
        }
    }
    public abstract object GetTarget(TAction handler);
}


public sealed class WeakEvent : WeakEventBase<Action> {
    public override object GetTarget(Action handler) => handler.Target;
    public void Invoke() {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke();
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1> : WeakEventBase<Action<T1>> {
    public override object GetTarget(Action<T1> handler) => handler.Target;
    public void Invoke(T1 arg1) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2> : WeakEventBase<Action<T1, T2>> {
    public override object GetTarget(Action<T1, T2> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3> : WeakEventBase<Action<T1, T2, T3>> {
    public override object GetTarget(Action<T1, T2, T3> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3, T4> : WeakEventBase<Action<T1, T2, T3, T4>> {
    public override object GetTarget(Action<T1, T2, T3, T4> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3, arg4);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3, T4, T5> : WeakEventBase<Action<T1, T2, T3, T4, T5>> {
    public override object GetTarget(Action<T1, T2, T3, T4, T5> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3, arg4, arg5);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3, T4, T5, T6> : WeakEventBase<Action<T1, T2, T3, T4, T5, T6>> {
    public override object GetTarget(Action<T1, T2, T3, T4, T5, T6> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3, T4, T5, T6, T7> : WeakEventBase<Action<T1, T2, T3, T4, T5, T6, T7>> {
    public override object GetTarget(Action<T1, T2, T3, T4, T5, T6, T7> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
public sealed class WeakEvent<T1, T2, T3, T4, T5, T6, T7, T8> : WeakEventBase<Action<T1, T2, T3, T4, T5, T6, T7, T8>> {
    public override object GetTarget(Action<T1, T2, T3, T4, T5, T6, T7, T8> handler) => handler.Target;
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
        foreach (var listener in _listeners) {
            if (listener.TryGetTarget(out var target)) target.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            // Remove the listener if the target has been garbage collected
            else _listeners = _listeners.Remove(listener);
        }
    }
}
*/
