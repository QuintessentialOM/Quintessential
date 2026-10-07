using Quintessential.Serialization;
using System;

namespace Quintessential.Components;

public interface IComponentHolder<THold, TComp> where TComp : IComponentBase<THold> where THold : IComponentHolder<THold, TComp> {
    public abstract bool RemoveComponent(Identifier toRemove);
    public abstract void AddComponent(TComp toAdd);
    public abstract void AddComponentSafe(Identifier id, Func<TComp> ctor);
    public abstract bool TryGetComponent(Identifier toGet, out TComp extension);
    public abstract TComp GetComponent(Identifier toGet);
    public abstract bool HasComponent(Identifier id);
}

public interface ISerializableComponentHolder<THold, TComp> : IComponentHolder<THold, TComp> where TComp : IComponentBase<THold> where THold : ISerializableComponentHolder<THold, TComp> {
    public static abstract void RegisterComponent(Identifier Id, Codec<TComp> codec);
}

////public class ComponentCatalogue<THold, TComp> : Dictionary<Identifier, Codec<TComp>> { }
