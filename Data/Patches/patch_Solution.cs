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
using System.Data;
using System.Globalization;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public class patch_Solution : ISerializableComponentHolder<patch_Solution, ISolutionComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, ISolutionComponent> Components;

    public void AddComponent(ISolutionComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Solution.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<ISolutionComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
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

    private static Dictionary<Identifier, Codec<ISolutionComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<ISolutionComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

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

    #region Serialization
    [MonoModIgnore]
    private Puzzle puzzle;
    internal void TestSetPuzzle(Puzzle p) {
        puzzle = p;
    }

    [MonoModIgnore]
    private extern PartsSnapshot CreatePartsSnapshot();
    [MonoModIgnore]
    private extern void OrderProgrammables();
    private static readonly Codec<Dictionary<Identifier, ISolutionComponent>> componentsCodec = CatalogueCodec<Identifier, ISolutionComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    private static readonly Codec<Dictionary<ScoreMetric, int>> scoreCodec = DictCodec<ScoreMetric, int>.Create(
        Codecs.INT, (index) => index.ToString(), str => Enum.Parse<ScoreMetric>(str)
    );
    private static readonly Codec<List<Part>> partsCodec = ListCodec<Part>.Create(Codecs.PART);
    internal static readonly Codec<Solution> SOLUTION = Codec<Solution>.Create(
        Codecs.STRING.Seal("PuzzleId", (Solution solution) => solution.GetPuzzle().puzzleId),
        Codecs.STRING.Seal("NameOnDisc", (Solution solution) => solution.nameOnDisk.ToString()),
        Codecs.STRING.Seal("CreationTime", (Solution solution) => solution.creationTime.ToInvariant()),
        Codecs.STRING.Seal("Name", (Solution solution) => solution.name),
        scoreCodec.Seal("Scores", (Solution solution) => solution.scores),
        partsCodec.Seal("Parts", (Solution solution) => [.. solution.CollectPartsAndSubparts().Where(part => !part.GetIsFixed() && !part.GetType().isSubpart)]),
        componentsCodec.Seal("Components", (Solution solution) => ((patch_Solution)(object)solution).Components).WithDefaut([]),
        (id, nameOnDisc, creationTime, name, scores, parts, components) => {
            if (!Puzzles.GetById(id).GetOrDefault(out Puzzle puzzle))
                throw new Exception($"Failed loading puzzle '{name}' puzzle with id '{id}' not found.");
            Solution solution = new(puzzle, name, SolutionNameOnDisk.Parse(nameOnDisc), DateTime.Parse(creationTime, CultureInfo.InvariantCulture)) {
                scores = scores,
                parts = parts.Where(part => puzzle.HasPermissionForPart(part.GetType())).ToList()
            };
            for (int i = 0; i < solution.parts.Count; i++) {
                solution.parts[0].SetupInputOutputFromSolution(solution, solution.parts[0].GetInputOutputIndex());
                solution.RepositionPart(solution.parts[0], solution.parts[0].GetHexPos()); // Replaces the part
            }
            ((patch_Solution)(object)solution).Components = components;
            solution.undoRedoBuffer.ClearAndAdd(((patch_Solution)(object)solution).CreatePartsSnapshot());
            ((patch_Solution)(object)solution).OrderProgrammables();
            return solution;
        }
    );

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
