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
using static Quintessential.Components.IMoleculeComponent;
using static Quintessential.CycleEvent;

public class patch_Molecule : ISerializableComponentHolder<patch_Molecule, IMoleculeComponent>, ISimCallbacks, IRecipeOutput, IRecipeInput {

    #region ComponentSystem
    private Dictionary<Identifier, IMoleculeComponent> Components;

    public void AddComponent(IMoleculeComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Molecule.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IMoleculeComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IMoleculeComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IMoleculeComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IMoleculeComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    private static readonly Dictionary<Identifier, Codec<IMoleculeComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IMoleculeComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    [MonoModIgnore] Dictionary<HexIndex, Atom> atoms;
    [MonoModIgnore] List<Bond> bonds;
    public CycleEventExecutionType CallbackType => (CycleEventExecutionType) 0b_0111_1111;
    public void OnCycleCallback(patch_Sim sim, CycleEventExecutionType executionType) {
        foreach (var atom in atoms) {
            ((patch_Atom)(object)atom.Value).OnCycleCallback(sim, executionType);
        }
        foreach (var bond in bonds) {
            ((patch_Bond)(object)bond).OnCycleCallback(sim, executionType);
        }
        foreach (var component in Components) {
            if (component.Value.CallbackType.HasFlag(executionType))
                component.Value.OnCycleCallback(sim, executionType);
        }
    }

    #endregion

    #region ComponentCalls

    #region ComponentCalls - Atom & Bond Adding/Removing

    public event OnAddBondDelegate OnAddBond;
    public event OnRemoveAtomDelegate OnRemoveAtom;
    public event OnAddAtomDelegate OnAddAtom;
    public event OnRemoveBondDelegate OnRemoveBond;
    public event OnReplaceAtomDelegate OnReplaceAtom;
    protected void OnRemoveBonds(List<patch_Bond> bonds) {
        foreach (var bond in bonds) {
            bond.OnRemoveFromMolecule(this);
            OnRemoveBond?.Invoke(bond);
        }
    }
    protected void OnAddBonds(List<patch_Bond> bonds) {
        foreach (var bond in bonds) {
            bond.OnRemoveFromMolecule(this);
            OnAddBond?.Invoke(bond);
        }
    }
    protected void OnAddSingleBond(patch_Bond bond) {
        OnAddBond?.Invoke(bond);
        bond.OnAddToMolecule(this);
    }
    protected void OnAddSingleAtom(patch_Atom atom, HexIndex hexPos) {
        OnAddAtom?.Invoke(atom, hexPos);
        atom.OnAddToMolecule(this, hexPos);
    }
    protected void OnRemoveSingleAtom(patch_Atom atom, HexIndex hexPos) {
        atom.OnRemoveFromMolecule(this, hexPos);
        OnRemoveAtom?.Invoke(hexPos);
    }
    protected void OnReplaceSingleAtom(patch_Atom atom, AtomType atomType, HexIndex hexPos) {
        atom.OnReplace(atomType);
        OnReplaceAtom?.Invoke(atomType, hexPos);
    }

    [MonoModILInject("ReplaceAtom")]
    static void PatchReplaceAtom(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        //MethodReference onCall = MonoModRule.Modder.FindType("Atom").Resolve().Methods.First(f => f.Name.Equals("OnReplace"));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnReplaceSingleAtom"));
        FieldReference atoms = null;
        MethodReference getItem = null;
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld(out atoms),
            instr => instr.MatchLdarg2(),
            instr => instr.MatchCallvirt(out getItem),
            instr => instr.MatchLdarg1());
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(atoms);
        cursor.EmitLdarg2();
        cursor.EmitCallvirt(getItem);
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(onCall);
    }

    [MonoModILInject("RemoveAtom")]
    static void PatchRemoveAtom(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCallAtom = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveSingleAtom"));
        MethodReference onCallBond = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveBonds"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Molecule", "atoms"));
        FieldReference atoms = cursor.Next.Operand as FieldReference;
        FieldReference hexpos = null;
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld(out hexpos));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Molecule", "bonds"));
        FieldReference bonds = cursor.Next.Operand as FieldReference;
        MethodReference isBondToHex = null;
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdftn(out isBondToHex));
        MethodReference predicateCtor = cursor.Next.Operand as MethodReference;

        MethodDefinition referenceCode = method.DeclaringType.Methods.First(f => f.Name.Equals("PatchRemoveAtomCodeReference"));
        ILCursor referenceCulsor = new(new ILContext(referenceCode));
        MethodReference getItemAtom = null;
        MethodReference findAllBonds = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out getItemAtom));
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchLdarg2());
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out findAllBonds));

        cursor.Index = 0;
        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc0());
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld(out _));

        //& this.OnRemoveSingleAtom(this.atoms[connectionChecker.hexPos], connectionChecker.hexPos);
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(atoms);
        cursor.EmitLdloc0();
        cursor.EmitLdfld(hexpos);
        cursor.EmitCallvirt(getItemAtom);
        cursor.EmitLdloc0();
        cursor.EmitLdfld(hexpos);
        cursor.EmitCall(onCallAtom);

        //& this.OnRemoveBonds(this.bonds.FindAll( new Predicate(IsBondToHex)));
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(bonds);
        cursor.EmitLdloc0();
        cursor.EmitLdftn(isBondToHex);
        cursor.EmitNewobj(predicateCtor);
        cursor.EmitCallvirt(findAllBonds);
        cursor.EmitCall(onCallBond);

        referenceCode.DeclaringType.Methods.Remove(referenceCode);
    }
    static void PatchRemoveAtomCodeReference(Dictionary<HexIndex, Atom> atoms, HexIndex pos, List<Bond> bonds, Predicate<Bond> predicate) {
        atoms[pos].ToString();
        bonds.FindAll(predicate);
    }

    [MonoModILInject("RemoveBond")]
    static void PatchRemoveBond(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCallBond = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveBonds"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Molecule", "bonds"));
        FieldReference bonds = cursor.Next.Operand as FieldReference;
        MethodReference isMatchingBond = null;
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdftn(out isMatchingBond));
        MethodReference predicateCtor = cursor.Next.Operand as MethodReference;

        MethodDefinition referenceCode = method.DeclaringType.Methods.First(f => f.Name.Equals("PatchRemoveBondCodeReference"));
        ILCursor referenceCulsor = new(new ILContext(referenceCode));
        MethodReference findAllBonds = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out findAllBonds));

        cursor.Index = 0;
        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc0());
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld(out _));
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld(out _));

        // this.OnRemoveBonds(this.bonds.FindAll( new Predicate(IsBondToHex)));
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(bonds);
        cursor.EmitLdloc0();
        cursor.EmitLdftn(isMatchingBond);
        cursor.EmitNewobj(predicateCtor);
        cursor.EmitCallvirt(findAllBonds);
        cursor.EmitCall(onCallBond);

        referenceCode.DeclaringType.Methods.Remove(referenceCode);
    }
    static void PatchRemoveBondCodeReference(List<Bond> bonds, Predicate<Bond> predicate) {
        bonds.FindAll(predicate);
    }

    [MonoModILInject("AddAtom")]
    static void PatchAddAtom(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddSingleAtom"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());

        //& this.OnAddToMolecule(atom, hexpos);
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(onCall);
    }

    [MonoModILInject("System.Boolean Molecule::AddBond(BondTypeEnum,HexIndex,HexIndex,Maybe`1<BondEffect>)")]
    static void PatchAddBond(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddSingleBond"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out var add) && add.Name == "Add");
        FieldReference bond = cursor.Previous.Previous.Operand as FieldReference;

        //& bondAdder.bond.OnAddToMolecule(this);
        cursor.EmitLdarg0();
        cursor.EmitLdloc(4);
        cursor.EmitLdfld(bond);
        cursor.EmitCall(onCall);
    }

    [MonoModILInject("MergeWith")]
    static void PatchMergeWithAddBond(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddBonds"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc0());
        FieldReference bond = cursor.Previous.Previous.Operand as FieldReference;

        //& molecule2.OnAddBonds(this.bonds);
        //& molecule2.OnAddBonds(molecule.bonds);
        cursor.EmitLdloc0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(bond);
        cursor.EmitCall(onCall);
        cursor.EmitLdloc0();
        cursor.EmitLdarg1();
        cursor.EmitLdfld(bond);
        cursor.EmitCall(onCall);
    }

    [MonoModILInject("Clone")]
    static void PatchCloneAddBondAndAtom(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCallBond = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddSingleBond"));
        MethodReference onCallAtom = method.DeclaringType.Methods.First(f => f.Name.Equals("OnAddSingleAtom"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Atom", "Clone"));
        MethodReference getKey = cursor.Previous.Previous.Previous.Previous.Operand as MethodReference;
        TypeReference atom = MonoModRule.Modder.FindType("Atom");

        MethodReference getValue = cursor.Previous.Previous.Operand as MethodReference;
        MethodReference cloneAtom = cursor.Previous.Operand as MethodReference;
        cursor.Index -= 3;
        cursor.RemoveRange(3);

        method.Body.Variables.Add(new VariableDefinition(atom));
        var clonedAtom = method.Body.Variables[^1];
        cursor.EmitLdloc(clonedAtom);
        Instruction saved = cursor.Next;

        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc0());
        cursor.EmitLdloca(2);
        cursor.EmitCall(getValue);
        cursor.EmitCallvirt(cloneAtom);
        cursor.EmitStloc(clonedAtom);
        cursor.Goto(saved, MoveType.After);

        cursor.EmitLdloc0();
        cursor.EmitLdloc(clonedAtom);
        cursor.EmitLdloca(2);
        cursor.EmitCall(getKey);
        cursor.EmitCall(onCallAtom);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Bond", "Clone"));
        MethodReference cloneBond = cursor.Previous.Operand as MethodReference;
        cursor.Index -= 2;
        cursor.RemoveRange(2);

        TypeReference bond = MonoModRule.Modder.FindType("Bond");
        method.Body.Variables.Add(new VariableDefinition(bond));
        var clonedBond = method.Body.Variables[^1];
        cursor.EmitLdloc(clonedBond);
        saved = cursor.Next;

        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc0());
        cursor.EmitLdloc(4);
        cursor.EmitCallvirt(cloneBond);
        cursor.EmitStloc(clonedBond);
        cursor.Goto(saved, MoveType.After);

        cursor.EmitLdloc0();
        cursor.EmitLdloc(clonedBond);
        cursor.EmitCall(onCallBond);
    }

    [MonoModILInject("RemoveRepeatAtoms")]
    static void PatchRemoveRepeatAtoms(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCallAtom = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRemoveSingleAtom"));
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld("Molecule", "atoms"));
        FieldReference atoms = cursor.Next.Next.Operand as FieldReference;

        MethodDefinition referenceCode = method.DeclaringType.Methods.First(f => f.Name.Equals("PatchRemoveRepeatAtomsCodeReference"));
        ILCursor referenceCulsor = new(new ILContext(referenceCode));
        MethodReference getItemAtom = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out getItemAtom));

        //& this.(this.atoms[hexPos], hexPos);
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(atoms);
        cursor.EmitLdloc1();
        cursor.EmitCallvirt(getItemAtom);
        cursor.EmitLdloc1();
        cursor.EmitCall(onCallAtom);

        referenceCode.DeclaringType.Methods.Remove(referenceCode);
    }
    static void PatchRemoveRepeatAtomsCodeReference(Dictionary<HexIndex, Atom> atoms, HexIndex pos) {
        atoms[pos].ToString();
    }

    #endregion

    #region ComponentCalls - Translation & Rotation

    public event OnRotateDelegate OnRotate;
    public event OnTranslateDelegate OnTranslate;

    [MonoModILInject("Rotate")]
    static void PatchRotate(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        FieldDefinition @delegate = method.DeclaringType.Fields.First(f => f.Name.Equals("OnRotate"));
        cursor.EmitLdarg0();
        cursor.EmitLdfld(@delegate);
        cursor.EmitDup();
        Instruction after = cursor.Next;
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCallvirt(@delegate.FieldType.Resolve().Methods.First(f => f.Name.Equals("Invoke")));
        cursor.Index -= 3;
        Instruction invoke = cursor.Next;
        cursor.Emit(OpCodes.Brtrue_S, invoke);
        cursor.EmitPop();
        cursor.Emit(OpCodes.Br_S, after);
    }

    [MonoModILInject("Translate")]
    static void PatchTranslate(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        FieldDefinition @delegate = method.DeclaringType.Fields.First(f => f.Name.Equals("OnTranslate"));
        cursor.EmitLdarg0();
        cursor.EmitLdfld(@delegate);
        cursor.EmitDup();
        Instruction after = cursor.Next;
        cursor.EmitLdarg1();
        cursor.EmitCallvirt(@delegate.FieldType.Resolve().Methods.First(f => f.Name.Equals("Invoke")));
        cursor.Index -= 2;
        Instruction invoke = cursor.Next;
        cursor.Emit(OpCodes.Brtrue_S, invoke);
        cursor.EmitPop();
        cursor.Emit(OpCodes.Br_S, after);
    }

    #endregion

    #region ComponentCalls - Molecule Handling

    internal void OnClone(ref patch_Molecule cloned) {
        foreach (var component in Components)
            component.Value.OnClone(ref cloned);
    }
    internal void OnMerge(ref patch_Molecule resoult, patch_Molecule other, bool asPrimary) {
        foreach (var component in Components)
            component.Value.OnMerge(ref resoult, other, asPrimary);
    }
    internal void OnSplit(List<patch_Molecule> resoult) {
        foreach (var component in Components)
            component.Value.OnSplit(resoult);
    }
    internal void OnRepeatAsMonomer(ref patch_Molecule resoult, HexIndex repetitionPos) {
        foreach (var component in Components)
            component.Value.OnRepeatAsMonomer(ref resoult, repetitionPos);
    }
    public void RenderMolecule(Vector2 offset, HexIndex hexPos, float rotationAngle, float opacityMultiplier, float height, float shadowStrength, bool isOutputRender, SolutionEditorBase solutionEditor) {
        using var enumerator = Components.GetEnumerator();
        CallRecursive();
        void CallRecursive() {
            if (enumerator.MoveNext()) {
                enumerator.Current.Value.OnRender(CallRecursive, ref offset, ref hexPos, ref rotationAngle, ref opacityMultiplier, ref height, ref shadowStrength, isOutputRender, solutionEditor);
            } else {
                patch_Editor.layer_0_RenderMolecule(this, offset, hexPos, rotationAngle, opacityMultiplier, height, shadowStrength, isOutputRender, solutionEditor);
            }
        }
    }

    [MonoModILInject("Clone")]
    static void PatchClone(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnClone"));
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdloc0(),
            instr => instr.MatchRet());
        Instruction oldTarget = cursor.Next;
        cursor.EmitLdarg0();
        Instruction newTarget = cursor.Previous;
        cursor.EmitLdloca(0);
        cursor.EmitCall(onCall);

        foreach (var v in cursor.Instrs.Where(v => v.Operand is Instruction t && t == oldTarget)) {
            v.Operand = newTarget;
        }
    }

    [MonoModILInject("MergeWith")]
    static void PatchMerge(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnMerge"));
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdloc0(),
            instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitLdloca(0);
        cursor.EmitLdarg1();
        cursor.EmitLdcI4(1);
        cursor.EmitCall(onCall);
        cursor.EmitLdarg1();
        cursor.EmitLdloca(0);
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(0);
        cursor.EmitCall(onCall);
    }

    [MonoModILInject("SplitIntoConnectedComponents")]
    static void PatchSplit(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnSplit"));
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdloc0(),
            instr => instr.MatchRet());
        cursor.Index++;
        cursor.Previous.OpCode = OpCodes.Ldarg_0;
        cursor.EmitLdloc0();
        cursor.EmitCall(onCall);
        cursor.EmitLdloc0();
    }

    [MonoModILInject("Molecule Molecule::RepeatMonomer(Molecule,HexIndex)")]
    static void PatchRepeatAsMonomer(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnRepeatAsMonomer"));
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdloc(5),
            instr => instr.MatchRet());
        cursor.EmitLdarg0();
        cursor.EmitLdloca(5);
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
    }

    #endregion

    #endregion

    #region Serialization
    [MonoModIgnore]
    public extern patch_Molecule GetMonomer();
    [MonoModIgnore]
    public static extern patch_Molecule RepeatMonomer(patch_Molecule monomer);

    private static readonly Codec<Dictionary<Identifier, IMoleculeComponent>> componentsCodec = CatalogueCodec<Identifier, IMoleculeComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    private static readonly Codec<Dictionary<HexIndex, Atom>> atomsCodec = DictCodec<HexIndex, Atom>.Create(Codecs.ATOM,
        (hexPos) => $"{hexPos.Q},{hexPos.R}",
        (str) => new HexIndex(int.Parse(str.Split(',')[0]), int.Parse(str.Split(',')[1]))
    );
    private static readonly Codec<List<Bond>> bondsCodec = ListCodec<Bond>.Create(Codecs.BOND);
    internal static readonly Codec<patch_Molecule> MOLECULE = Codec<patch_Molecule>.Create(
        Codecs.M_LOCSTRING.Seal("Name", (patch_Molecule molec) => ((Molecule)(object)molec).GetMonomer().displayName.GetOrDefault(LocString.emptyString)).WithDefaut(LocString.emptyString),
        atomsCodec.Seal("Atoms", (patch_Molecule molec) => molec.GetMonomer().atoms).WithDefaut([]),
        bondsCodec.Seal("Bonds", (patch_Molecule molec) => molec.GetMonomer().bonds).WithDefaut([]),
        componentsCodec.Seal("Components", (patch_Molecule molec) => molec.GetMonomer().Components).WithDefaut([]),
        (name, atoms, bonds, components) => {
            patch_Molecule molec = (patch_Molecule)(object)new Molecule();
            ((Molecule)(object)molec).displayName = name;
            molec.atoms = atoms;
            molec.bonds = bonds;
            molec.Components = components;
            return RepeatMonomer(molec);
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
