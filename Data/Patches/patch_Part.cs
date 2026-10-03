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

public class patch_Part : ISerializableComponentHolder<patch_Part, IPartComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, IPartComponent> Components;

    public void AddComponent(IPartComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Part.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IPartComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IPartComponent extension) {
        return Components.TryGetValue(toGet, out extension);
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

    private static readonly Dictionary<Identifier, Codec<IPartComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IPartComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

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
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Part", "SetRotation"));
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

    #region Serialization

    [MonoModIgnore]
    private extern void SetInputOutputIndex(int inputOutputIndex);
    [MonoModIgnore]
    private extern void SetRotation(HexRotation rotation);

    private static readonly Codec<Dictionary<Identifier, IPartComponent>> componentsCodec = CatalogueCodec<Identifier, IPartComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    private static readonly Codec<Dictionary<int,InstructionType>> programCodec = DictCodec<int, InstructionType>.Create(
        Codecs.INSTRTYPE, (index) => index.ToString(), str => int.Parse(str)
    );
    internal static readonly Codec<Part> PART = Codec<Part>.Create(
        Codecs.ID.Seal("Id", (Part part) => part.GetType().Id),
        Codecs.BOOL.Seal("IsFixed", (Part part) => part.GetIsFixed()).WithDefaut(false),
        Codecs.HEXINDEX.Seal("Pos", (Part part) => part.GetHexPos()),
        Codecs.INT.Seal("Length", (Part part) => part.GetArmLength()).WithDefaut(1).WriteDefautIf(part => PartTag.PartTags["om:arm"].HasPart(part)),
        Codecs.INT.Seal("Rotation", (Part part) => part.GetRotation().GetNumberOfTurns()).WithDefaut(0).WriteDefautIf(part => part.GetType().canRotateInEditor),
        Codecs.INT.Seal("IOIndex", (Part part) => part.GetInputOutputIndex()).WithDefaut(0).WriteDefautIf(part => PartTag.PartTags["om:inputoutput"].HasPart(part)),
        Codecs.INT.Seal("ProgramIndex", (Part part) => part.programIndex).WithDefaut(0).WriteDefautIf(part => PartTag.PartTags["om:programmable"].HasPart(part)),
        programCodec.Seal("Program", (Part part) => new Dictionary<int, InstructionType>(part.program.method_902().ToList().Select(indexed => KeyValuePair.Create(indexed.index, indexed.instruction)))).WithDefaut([]).WriteDefautIf(part => PartTag.PartTags["om:programmable"].HasPart(part)),
        Codecs.LIST_HEXINDEX.Seal("Track", (Part part) => part.GetTrack()?.ToList() ?? []).WithDefaut([]),
        Codecs.INT.Seal("ConduitId", (Part part) => part.conduitId).WithDefaut(0).WriteDefautIf(part => part.GetType().Id == "om:conduit"),
        Codecs.LIST_HEXINDEX.Seal("Conduit", (Part part) => part?.GetConduitHexes() ?? []).WithDefaut([]),
        componentsCodec.Seal("Components", (Part part) => ((patch_Part)(object)part).Components.Where(pair => RegisteredComponents[pair.Key] != null).ToDictionary()).WithDefaut([]),
        (id, isFixed, pos, lenght, rotation, ioIndex, programIndex, program, track, conduitId, conduit, components) => {
            Part part = new(PartTypes.GetById(id).GetValue(), isFixed);
            part.SetHexPosAndUpdate(pos);
            part.SetAllowedLength(lenght);
            ((patch_Part)(object)part).SetRotation(new HexRotation(rotation));
            ((patch_Part)(object)part).SetInputOutputIndex(ioIndex); // TODO setup from solution
            part.programIndex = programIndex;
            foreach (var instruction in program)
                part.program.SetInstruction(instruction.Key, instruction.Value);
            if (track.Count != 0) {
                part.RemoveTrackBack();
                foreach (var hexPos in track)
                    part.AddTrackBack(hexPos);
            } 
            part.conduitId = conduitId;
            if(conduit.Count != 0) part.InitConduit(conduit);
            foreach (var component in components) {
                if (((patch_Part)(object)part).HasComponent(component.Key))
                    ((patch_Part)(object)part).RemoveComponent(component.Key);
                ((patch_Part)(object)part).AddComponent(component.Value);
            }
            return part;
        }
    );

    #endregion

    #region Ctor
    public static event Action<patch_Part> OnCreate;
    internal static Dictionary<Identifier, List<Func<patch_Part, IPartComponent>>> CtorsByID = [];

    public void InitObjectsInCtor() {
        Components = [];
    }
    public void AfterCreate() {
        if (CtorsByID.TryGetValue(((Part)(object)this).GetType().Id, out var componentCtors))
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
