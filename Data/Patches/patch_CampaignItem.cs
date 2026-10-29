using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


public class patch_CampaignItem : ISerializableComponentHolder<patch_CampaignItem, ICampaignItemComponent> {

    #region ComponentSystem
    internal Dictionary<Identifier, ICampaignItemComponent> Components;
    public void AddComponent(ICampaignItemComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            RegisterComponent(toAdd.Id, null);
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Puzzle.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }

    public void AddComponentSafe(Identifier id, Func<ICampaignItemComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            RegisterComponent(id, null);
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out ICampaignItemComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public ICampaignItemComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public ReadOnlyDictionary<Identifier, ICampaignItemComponent> GetComponents() {
        return new ReadOnlyDictionary<Identifier, ICampaignItemComponent>(Components);
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out ICampaignItemComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    internal static Dictionary<Identifier, Codec<ICampaignItemComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<ICampaignItemComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    #endregion

    #region ComponentCalls

    internal void OnSelected(ref bool handleAsVanilla) {
        Identifier[] componentIDs = [.. Components.Keys];
        for (int i = 0; i < componentIDs.Length; i++) {
            if (Components.TryGetValue(componentIDs[i], out var component))
                component.OnSelected(ref handleAsVanilla);
        }
    }

    #endregion

    #region Ctor

    public void InitObjectsInCtor() {
        Components = [];
    }

    [MonoModILInject(".ctor")]
    static void PatchCtor(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Sim init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Sim init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        MethodReference init = method.DeclaringType.Methods.First(f => f.Name.Equals("InitObjectsInCtor"));
        cursor.EmitLdarg0();
        cursor.EmitCall(init);
    }

    #endregion
}
