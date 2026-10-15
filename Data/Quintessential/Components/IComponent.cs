
namespace Quintessential.Components;

public interface IComponentBase<THold> {
    public Identifier Id { get; }
}
public interface IComponent<THold> : IComponentBase<THold> {
    public abstract void OnBind(THold self);
    public abstract void OnUnbind(THold self);
}
public interface IComponent<THold, TArg1> : IComponentBase<THold> {
    public abstract void OnBind(THold self, TArg1 arg1);
    public abstract void OnUnbind(THold self, TArg1 arg1);
}
public interface IComponent<THold, TArg1, TArg2> : IComponentBase<THold> {
    public abstract void OnBind(THold self, TArg1 arg1, TArg2 arg2);
    public abstract void OnUnbind(THold self, TArg1 arg1, TArg2 arg2);
}
public interface IComponent<THold, TArg1, TArg2, TArg3> : IComponentBase<THold> {
    public abstract void OnBind(THold self, TArg1 arg1, TArg2 arg2, TArg3 arg3);
    public abstract void OnUnbind(THold self, TArg1 arg1, TArg2 arg2, TArg3 arg3);
}
public interface IComponent<THold, TArg1, TArg2, TArg3, TArg4> : IComponentBase<THold> {
    public abstract void OnBind(THold self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
    public abstract void OnUnbind(THold self, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
}

