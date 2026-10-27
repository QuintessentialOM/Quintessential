using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


public class patch_JournalVolume : ISerializableComponentHolder<patch_JournalVolume, IJournalVolumeComponent> {

    #region ComponentSystem
    internal Dictionary<Identifier, IJournalVolumeComponent> Components;
    public void AddComponent(IJournalVolumeComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            RegisterComponent(toAdd.Id, null);
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Puzzle.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }

    public void AddComponentSafe(Identifier id, Func<IJournalVolumeComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            RegisterComponent(id, null);
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IJournalVolumeComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IJournalVolumeComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public ReadOnlyDictionary<Identifier, IJournalVolumeComponent> GetComponents() {
        return new ReadOnlyDictionary<Identifier, IJournalVolumeComponent>(Components);
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IJournalVolumeComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    internal static Dictionary<Identifier, Codec<IJournalVolumeComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IJournalVolumeComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    #endregion
}
