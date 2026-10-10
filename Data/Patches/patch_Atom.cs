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

public class patch_Atom : ISerializableComponentHolder<patch_Atom, IAtomComponent>, ISimCallbacks {

    #region ComponentSystem
    private Dictionary<Identifier, IAtomComponent> Components;

    public void AddComponent(IAtomComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            RegisterComponent(toAdd.Id, null);
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Atom.");
        toAdd.OnBind(this, Molecule);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IAtomComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            RegisterComponent(id, null);
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IAtomComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IAtomComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IAtomComponent value))
            value.OnUnbind(this, Molecule);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    private static readonly Dictionary<Identifier, Codec<IAtomComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IAtomComponent> codec) {
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

    internal void OnClone(ref patch_Atom cloned) {
        foreach (var component in Components)
            component.Value.OnClone(ref cloned);
    }
    internal void OnReplace(AtomType newType) {
        foreach (var component in Components)
            component.Value.OnReplace(newType);
        if (CtorsByIDAfterReplace.TryGetValue(newType.Id, out var componentCtors))
            foreach (var item in componentCtors)
                AddComponent(item(this, newType));
    }
    internal void OnRemoveFromMolecule(patch_Molecule molecule, HexIndex hexPos) {
        foreach (var component in Components)
            component.Value.OnRemoveFromMolecule(molecule, hexPos);
        Molecule = null;
    }
    internal void OnAddToMolecule(patch_Molecule molecule, HexIndex hexPos) {
        Molecule = molecule;
        foreach (var component in Components)
            component.Value.OnAddToMolecule(molecule, hexPos);
    }
    public void RenderAtom(Vector2 translation, float scaleMultiplier, float opacityMultiplier, float height, float shadowStrength, float shadowOffset, float shadowAngle, Texture shadow, Texture overlayEffect, bool isOutputRender, SolutionEditorBase solutionEditor) {
        AtomType atomType = ((Atom)(object)this).atomType;
        using var enumerator = Components.GetEnumerator();
        CallRecursive();
        void CallRecursive() {
            if (enumerator.MoveNext()) {
                enumerator.Current.Value.OnRender(CallRecursive, ref atomType, ref translation, ref scaleMultiplier, ref opacityMultiplier, ref height, ref shadowStrength, ref shadowOffset, ref shadowAngle, shadow, overlayEffect, isOutputRender, solutionEditor);
            } else {
                Editor.RenderAtom(atomType, translation, scaleMultiplier, opacityMultiplier, height, shadowStrength, shadowOffset, shadowAngle, shadow, overlayEffect, isOutputRender);
            }
        }
    }

    #endregion

    #region Serialization

    private static readonly Codec<Dictionary<Identifier, IAtomComponent>> componentsCodec = CatalogueCodec<Identifier, IAtomComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    internal static readonly Codec<Atom> ATOM = Codec<Atom>.Create(
        Codecs.ATOMTYPE.Seal("Type", (Atom atom) => atom.atomType),
        componentsCodec.Seal("Components", (Atom atom) => ((patch_Atom)(object)atom).Components.Where(pair => RegisteredComponents[pair.Key] != null).ToDictionary()).WithDefaut([]),
        (type, components) => {
            Atom atom = new(type);
            foreach (var component in components) {
                if (((patch_Atom)(object)atom).HasComponent(component.Key))
                    ((patch_Atom)(object)atom).RemoveComponent(component.Key);
                ((patch_Atom)(object)atom).AddComponent(component.Value);
            }
            return atom;
        }
    );

    #endregion

    #region Ctor
    public static event Action<patch_Atom> OnCreate;
    internal static Dictionary<Identifier, List<Func<patch_Atom, IAtomComponent>>> CtorsByID = [];
    internal static Dictionary<Identifier, List<Func<patch_Atom, AtomType, IAtomComponent>>> CtorsByIDAfterReplace = [];

    public void InitObjectsInCtor() {
        Components = [];
    }
    public void AfterCreate() {
        if (CtorsByID.TryGetValue(((Atom)(object)this).atomType.Id, out var componentCtors))
            foreach (var item in componentCtors)
                AddComponent(item(this));
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
