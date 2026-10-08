using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using static Quintessential.CycleEvent;

public class patch_PartSimState : IComponentHolder<patch_PartSimState, ISimStateComponent>, ISimCallbacks {

    #region ComponentSystem
    private Dictionary<Identifier, ISimStateComponent> Components;
    protected Part Part;
    protected Sim Sim;

    public void AddComponent(ISimStateComponent toAdd) {
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this PartSimState.");
        toAdd.OnBind(this, Part, Sim);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<ISimStateComponent> ctor) {
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out ISimStateComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public ISimStateComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out ISimStateComponent value))
            value.OnUnbind(this, Part, Sim);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    public CycleEventExecutionType CallbackType => (CycleEventExecutionType)0b_0111_1111;
    public void OnCycleCallback(patch_Sim sim, CycleEventExecutionType executionType) {
        foreach (var component in Components) {
            if (component.Value.CallbackType.HasFlag(executionType))
                component.Value.OnCycleCallback(sim, executionType);
        }
    }

    #endregion

    #region ComponentCalls

    internal void OnReset() {
        foreach (var component in Components)
            component.Value.OnReset();
    }
    internal void OnResetForGlyphs() {
        foreach (var component in Components)
            component.Value.OnResetForGlyphs();
    }
    internal void OnSpawnMolecules(HashSet<HexIndex> occupied) {
        foreach (var component in Components)
            component.Value.OnSpawnMolecules(occupied);
    }
    internal void OnInstruction(InstructionType instruction, Maybe<int> index, InstructionCycleState instructionState) {
        foreach (var component in Components)
            component.Value.OnInstruction(instruction, index, instructionState);
    }
    internal void OnGrabStateChange(Maybe<Molecule> molecule, bool newState) {
        foreach (var component in Components)
            component.Value.OnGrabStateChange(molecule, newState);
    }

    #endregion

    #region Ctor
    internal static Dictionary<Identifier, List<Func<patch_PartSimState, Part, Sim?, ISimStateComponent>>> CtorsByID = [];

    public void InitObjectsInCtor(Part? part, Sim? sim) {
        Components = [];
        Part = part;
        Sim = sim;
    }
    public void AfterCreate(Part? part, Sim? sim) {
        if (part != null && CtorsByID.TryGetValue(part.GetType().Id, out var componentCtors))
            foreach (var item in componentCtors)
                AddComponent(item(this, part, sim));
    }

    [MonoModILInject(".ctor")]
    static void PatchSimCtorArgs(MethodDefinition method, CustomAttribute attribute) {
        TypeReference sim = MonoModRule.Modder.FindType("Sim");
        TypeReference part = MonoModRule.Modder.FindType("Part");
        MethodDefinition getState = part.Resolve().Methods.First(f => f.Name.Equals("GetSimState"));
        if (!getState.HasBody) {
            throw new Exception("Unable to patch Sim Construct. (no body)");
        }
        method.Parameters.Add(new ParameterDefinition("part", ParameterAttributes.None, part));
        method.Parameters.Add(new ParameterDefinition("sim", ParameterAttributes.None, sim));

        ILCursor cursor = new(new ILContext(getState));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchNewobj(method.DeclaringType.FullName, method.Name));
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.Remove();
        cursor.EmitNewobj(method);
    }

    [MonoModILInject(".ctor")]
    static void PatchCtor(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Sim init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Sim init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        MethodReference init = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("InitObjectsInCtor"));
        MethodReference after = method.DeclaringType.Methods.First(f => f.Name.Equals("AfterCreate"));

        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(init);

        cursor.Index = cursor.Instrs.Count;
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(after);
    }

    #endregion
}
