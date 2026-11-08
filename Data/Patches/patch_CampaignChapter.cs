using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


public class patch_CampaignChapter : ISerializableComponentHolder<patch_CampaignChapter, IChapterComponent> {

    #region ComponentSystem
    internal Dictionary<Identifier, IChapterComponent> Components;
    public void AddComponent(IChapterComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            RegisterComponent(toAdd.Id, null);
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Puzzle.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }

    public void AddComponentSafe(Identifier id, Func<IChapterComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            RegisterComponent(id, null);
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IChapterComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IChapterComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public ReadOnlyDictionary<Identifier, IChapterComponent> GetComponents() {
        return new ReadOnlyDictionary<Identifier, IChapterComponent>(Components);
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IChapterComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    internal static Dictionary<Identifier, Codec<IChapterComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IChapterComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    #endregion
}
