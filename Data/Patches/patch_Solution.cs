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

public class patch_Solution : IComponentHolder<patch_Solution, ISolutionComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, ISolutionComponent> Components;

    public void AddComponent(ISolutionComponent toAdd) {
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Solution.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<ISolutionComponent> ctor) {
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out ISolutionComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public ISolutionComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out ISolutionComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    #endregion

    #region ComponentCalls

    internal void OnPlacementCheck(ref bool origReturnVal, Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, ref string? errorMessage) {
        foreach (var component in Components)
            component.Value.OnPlacementCheck(ref origReturnVal, part, otherInputOutputIndex, offset, rotationOffset, ref errorMessage);
    }
    internal void OnCreateSnapshot(ref PartsSnapshot created) {
        foreach (var component in Components)
            component.Value.OnCreateSnapshot(ref created);
    }
    internal void OnRestoreSnapshot(PartsSnapshot restored) {
        foreach (var component in Components)
            component.Value.OnRestoreSnapshot(restored);
    }
    internal void OnMakeCopyOfSolution(ref Solution copy) {
        foreach (var component in Components)
            component.Value.OnMakeCopyOfSolution(ref copy);
    }

    [MonoModILInject("IsAllowedPlacement")]
    static void PatchRotateBy(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnPlacementCheck"));
        method.Body.Variables.Add(new VariableDefinition(method.ReturnType));
        VariableDefinition cloned = method.Body.Variables[^1];
        while (cursor.TryGotoNext(instr => instr.MatchRet())) {
            cursor.EmitStloc(cloned);
            cursor.EmitLdarg0();
            cursor.EmitLdloca(cloned);
            cursor.EmitLdarg1();
            cursor.EmitLdarg2();
            cursor.EmitLdarg3();
            cursor.EmitLdarg(4);
            cursor.EmitLdarg(5);
            cursor.EmitCall(onCall);
            cursor.EmitLdloc(cloned);
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchRet());
        }
    }
    [MonoModILInject("CreatePartsSnapshot")]
    static void PatchSaveSnapshot(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnCreateSnapshot"));
        cursor.GotoNext(instr => instr.MatchRet());
        method.Body.Variables.Add(new VariableDefinition(method.ReturnType));
        VariableDefinition cloned = method.Body.Variables[^1];
        cursor.EmitStloc(cloned);
        cursor.EmitLdarg0();
        cursor.EmitLdloca(cloned);
        cursor.EmitCall(onCall);
        cursor.EmitLdloc(cloned);
    }
    [MonoModILInject("RestorePartsSnapshot")]
    static void RestorePartsSnapshot(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRestoreSnapshot"));
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("MakeCopyOfSolution")]
    static void PatchMakeCopy(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnMakeCopyOfSolution"));
        cursor.GotoNext(instr => instr.MatchRet());
        cursor.Index--;
        cursor.EmitLdarg0();
        cursor.EmitLdloca(1);
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

    #endregion
}
