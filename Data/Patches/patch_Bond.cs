using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using static Quintessential.CycleEvent;

public class patch_Bond : ISerializableComponentHolder<patch_Bond, IBondComponent>, ISimCallbacks, IRecipeOutput {

    #region ComponentSystem
    private Dictionary<Identifier, IBondComponent> Components;

    public void AddComponent(IBondComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Bond.");
        toAdd.OnBind(this, Molecule);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IBondComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IBondComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IBondComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IBondComponent value))
            value.OnUnbind(this, Molecule);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    private static readonly Dictionary<Identifier, Codec<IBondComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IBondComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    public CycleEventExecutionType CallbackType => (CycleEventExecutionType)0b_0111_1111;
    public void OnCycleCallback(patch_Sim sim, CycleEventExecutionType executionType) {
        foreach (var component in Components) {
            if (component.Value.CallbackType.HasFlag(executionType))
                component.Value.OnCycleCallback(sim, executionType);
        }
    }

    #endregion

    #region ComponentCalls

    protected patch_Molecule Molecule;

    [MonoModILInject("Clone")]
    static void PatchClone(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnClone"));
        cursor.GotoNext(instr => instr.MatchRet());
        method.Body.Variables.Add(new VariableDefinition(method.DeclaringType));
        VariableDefinition cloned = method.Body.Variables[^1];
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
    }

    internal void OnClone(ref patch_Bond cloned) {
        foreach (var component in Components)
            component.Value.OnClone(ref cloned);
    }
    internal void OnRemoveFromMolecule(patch_Molecule molecule) {
        foreach (var component in Components)
            component.Value.OnRemoveFromMolecule(molecule);
        Molecule = null;
    }
    internal void OnAddToMolecule(patch_Molecule molecule) {
        Molecule = molecule;
        foreach (var component in Components)
            component.Value.OnAddToMolecule(molecule);
    }
    public void RenderBond(Vector2 offset, HexIndex hexOffset, float rotationAngle, float opacityMultiplier, float height, SolutionEditorBase solutionEditor) {
        using var enumerator = Components.GetEnumerator();
        CallRecursive();
        void CallRecursive() {
            if (enumerator.MoveNext()) {
                enumerator.Current.Value.OnRender(CallRecursive, ref offset, ref hexOffset, ref rotationAngle, ref opacityMultiplier, ref height, solutionEditor);
            } else {
                Editor.RenderBond((Bond)(object)this, offset, hexOffset, rotationAngle, opacityMultiplier, height, solutionEditor);
            }
        }
    }

    #endregion

    #region Serialization

    private static readonly Codec<Dictionary<Identifier, IBondComponent>> componentsCodec = CatalogueCodec<Identifier, IBondComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    private static readonly Codec<BondTypeEnum> bondTypeCodec = EnumCodec<BondTypeEnum>.Create();
    internal static readonly Codec<Bond> BOND = Codec<Bond>.Create(
        bondTypeCodec.Seal("Type", (Bond bond) => bond.type),
        Codecs.HEXINDEX.Seal("Pos1", (Bond bond) => bond.hexPos1),
        Codecs.HEXINDEX.Seal("Pos2", (Bond bond) => bond.hexPos2),
        componentsCodec.Seal("Components", (Bond bond) => ((patch_Bond)(object)bond).Components.Where(pair => RegisteredComponents[pair.Key] != null).ToDictionary()).WithDefaut([]),
        (type, pos1, pos2, components) => {
            Bond bond = new(type, pos1, pos2);
            foreach (var component in components) {
                if (((patch_Bond)(object)bond).HasComponent(component.Key))
                    ((patch_Bond)(object)bond).RemoveComponent(component.Key);
                ((patch_Bond)(object)bond).AddComponent(component.Value);
            }
            return bond;
        }
    );

    #endregion

    #region Ctor
    public static event Action<patch_Bond> OnCreate;
    internal static Dictionary<Identifier, List<Func<patch_Bond, IBondComponent>>> CtorsByID = [];

    public void InitObjectsInCtor() {
        Components = [];
    }
    public void AfterCreate() {
        //if (CtorsByID.TryGetValue(((Bond)(object)this).Id, out var componentCtors))
        //    foreach (var item in componentCtors)
        //        AddComponent(item(this));
        OnCreate?.Invoke(this);
    }

    [MonoModILInject(".ctor")]
    static void PatchCtor(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Sim init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Sim init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        MethodReference init = method.DeclaringType.Methods.First(f => f.Name.Equals("InitObjectsInCtor"));
        MethodReference after = method.DeclaringType.Methods.First(f => f.Name.Equals("AfterCreate"));
        cursor.EmitLdarg0();
        cursor.EmitCall(init);

        cursor.Index = cursor.Instrs.Count;
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitCall(after);
    }

    #endregion
}
