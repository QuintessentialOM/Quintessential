using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using System;
using System.Collections.Generic;
using System.Linq;

public class patch_Part : IComponentHolder<patch_Part, IPartComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, IPartComponent> Components;

    public void AddComponent(IPartComponent toAdd) {
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Part.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public bool TryGetComponent(Identifier toGet, out IPartComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public void AddComponentSafe(Identifier id, Func<IPartComponent> ctor) {
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public IPartComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IPartComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    #endregion

    #region ComponentCalls

    internal void OnClone(ref patch_Part cloned, Solution solution) {
        foreach (var component in Components)
            component.Value.OnClone(ref cloned, solution);
    }
    internal void OnReset() {
        foreach (var component in Components)
            component.Value.OnReset();
    }
    internal void OnGetMoleculeForInputOutput(ref Molecule resoult, Solution solution) {
        foreach (var component in Components)
            component.Value.OnGetMoleculeForInputOutput(ref resoult, solution);
    }
    internal void OnAddTrackFront() {
        foreach (var component in Components)
            component.Value.OnAddTrackFront();
    }
    internal void OnAddTrackBack() {
        foreach (var component in Components)
            component.Value.OnAddTrackBack();
    }
    internal void OnRemoveTrackFront() {
        foreach (var component in Components)
            component.Value.OnRemoveTrackFront();
    }
    internal void OnRemoveTrackBack() {
        foreach (var component in Components)
            component.Value.OnRemoveTrackBack();
    }
    internal void OnSetHexPos(HexIndex newPos) {
        foreach (var component in Components)
            component.Value.OnSetHexPos(newPos);
    }
    internal void OnRotateBy(Solution solution, HexRotation rotation) {
        foreach (var component in Components)
            component.Value.OnRotateBy(solution, rotation);
    }
    internal void OnSetRotation(Solution solution, HexRotation rotation) {
        foreach (var component in Components)
            component.Value.OnSetRotation(solution, rotation);
    }
    internal void OnSetLength(int newLenght) {
        foreach (var component in Components)
            component.Value.OnSetLength(newLenght);
    }

    [MonoModILInject("Clone")]
    static void PatchClone(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnClone"));
        cursor.GotoNext(instr => instr.MatchRet());
        method.Body.Variables.Add(new VariableDefinition(method.ReturnType));
        VariableDefinition cloned = method.Body.Variables[^1];
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
    }
    [MonoModILInject("ResetTypeById")]
    static void PatchReset(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnReset"));
        cursor.EmitLdarg0();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("GetMoleculeForInputOutput")]
    static void PatchGetInputOutput(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnGetMoleculeForInputOutput"));
        method.Body.Variables.Add(new VariableDefinition(method.ReturnType));
        VariableDefinition cloned = method.Body.Variables[^1];
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
        cursor.Index++;
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
        cursor.Index++;
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
    }
    [MonoModILInject("AddTrackFront")]
    static void PatchAddTrackFront(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddTrackFront"));
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("AddTrackBack")]
    static void PatchAddTrackBack(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddTrackBack"));
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("RemoveTrackFront")]
    static void PatchRemoveTrackFront(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveTrackFront"));
        cursor.EmitLdarg0();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("RemoveTrackBack")]
    static void PatchRemoveTrackBack(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveTrackBack"));
        cursor.EmitLdarg0();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("SetHexPosAndUpdate")]
    static void PatchSetPos(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnSetHexPos"));
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("RotateBy")]
    static void PatchRotateBy(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRotateBy"));
        cursor.GotoNext(instr => instr.MatchCallvirt("Part", "GetType"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("System.Void Part::SetRotation(Solution,HexRotation)")]
    static void PatchSetRotation(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnSetRotation"));
        cursor.GotoNext(instr => instr.MatchCallvirt("Part", "SetRotation"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(onCall);
        cursor.GotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        cursor.GotoNext(instr => instr.MatchCallvirt("Part", "SetRotation"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("SetAllowedLength")]
    static void PatchSetLength(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnSetLength"));
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
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

    // # Related to the .ctor of PartSimStates, adding arguments
    [MonoModILInject("GetSimState")]
    static void PatchSimConstructArg(MethodDefinition method, CustomAttribute attribute) {
        TypeReference sim = MonoModRule.Modder.FindType("Sim");
        MethodDefinition construct = sim.Resolve().Methods.First(f => f.Name.Equals("ConstructSimulation"));
        MethodDefinition emptyGet = MonoModRule.Modder.FindType("EmptySim").Resolve().Methods.First(f => f.Name.Equals("GetSimState"));
        if (!construct.HasBody) {
            throw new Exception("Unable to patch Sim Construct. (no body)");
        }
        method.Parameters.Add(new ParameterDefinition("sim", ParameterAttributes.None, sim));

        ILCursor cursor = new(new ILContext(construct));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt(method.DeclaringType.FullName, method.Name));
        cursor.EmitLdloc0();
        cursor.Remove();
        cursor.EmitCallvirt(method);

        cursor = new(new ILContext(emptyGet));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt(method.DeclaringType.FullName, method.Name));
        cursor.EmitLdnull();
        cursor.Remove();
        cursor.EmitCallvirt(method);
    }

    #endregion
}
