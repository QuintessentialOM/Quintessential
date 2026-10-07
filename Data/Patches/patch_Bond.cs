using MonoMod;
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
                patch_Editor.layer_0_RenderBond(this, offset, hexOffset, rotationAngle, opacityMultiplier, height, solutionEditor);
            }
        }
    }

    #endregion

    #region CustomBonds
    private List<BondType> bondTypes;

    [Obsolete]
    [MonoModIgnore] public BondTypeEnum type;
    [MonoModIgnore] public HexIndex hexPos1;
    [MonoModIgnore] public HexIndex hexPos2;
    [MonoModIgnore] public List<BondEffect> effects;

    [Obsolete]
    [MonoModReplace]
    [MonoModConstructor]
    public patch_Bond(BondTypeEnum type, HexIndex hexPos1, HexIndex hexPos2) {
        effects = [];
        this.type = type;
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        bondTypes = [];
        if ((type & BondTypeEnum.Standard) == BondTypeEnum.Standard) bondTypes.Add("om:standard");
        if ((type & BondTypeEnum.Prisma0) == BondTypeEnum.Prisma0) bondTypes.Add("om:prisma0");
        if ((type & BondTypeEnum.Prisma1) == BondTypeEnum.Prisma1) bondTypes.Add("om:prisma1");
        if ((type & BondTypeEnum.Prisma2) == BondTypeEnum.Prisma2) bondTypes.Add("om:prisma2");
        bondTypes = [.. bondTypes.OrderByDescending(type => type.renderPriority)];
        InitObjectsInCtor();
        AfterCreate();
    }

    [MonoModConstructor]
    public patch_Bond(BondType bondType, HexIndex hexPos1, HexIndex hexPos2) {
        effects = new List<BondEffect>();
        type = (BondTypeEnum)BondTypes.GetBondIndex(bondType.Id);
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        bondTypes = [bondType];
        InitObjectsInCtor();
        AfterCreate();
    }

    [MonoModConstructor]
    public patch_Bond(List<BondType> bondTypes, HexIndex hexPos1, HexIndex hexPos2) {
        effects = new List<BondEffect>();
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        type = bondTypes.Count > 0 ? (BondTypeEnum)BondTypes.GetBondIndex(bondTypes[0].Id) : BondTypeEnum.None;
        InitObjectsInCtor();
        AfterCreate();
    }

    [MonoModReplace]
    public patch_Bond Clone() {
        patch_Bond bond = new([..bondTypes], hexPos1, hexPos2) {
            effects = [.. effects]
        };
        OnClone(ref bond);
        return bond;
    }

    // Returns true if it was added
    internal bool AddTypeSafe(BondType type) {
        if (bondTypes.Contains(type)) return false;
        int i = 0;
        while (bondTypes.Count > i && bondTypes[i].renderPriority > type.renderPriority) i++;
        bondTypes.Insert(i, type);
        this.type = this.type | (BondTypeEnum)BondTypes.GetBondIndex(type.Id);
        return true;
    }

    // Returns true if it was removed
    internal bool RemoveTypeSafe(BondType type) {
        if (!bondTypes.Contains(type)) return false;
        bondTypes.Remove(type);
        this.type = this.type & ~(BondTypeEnum)BondTypes.GetBondIndex(type.Id);
        return true;
    }
    public IReadOnlyList<BondType> GetBondTypes() => bondTypes;

    #endregion

    #region Serialization

    private static readonly Codec<Dictionary<Identifier, IBondComponent>> componentsCodec = CatalogueCodec<Identifier, IBondComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    internal static readonly Codec<Bond> BOND = Codec<Bond>.Create(
        Codecs.LIST_ID.Seal("Types", (Bond bond) => [.. ((patch_Bond)(object)bond).bondTypes.Select(bondType => bondType.Id)]).WithDefaut([]),
        Codecs.HEXINDEX.Seal("Pos1", (Bond bond) => bond.hexPos1),
        Codecs.HEXINDEX.Seal("Pos2", (Bond bond) => bond.hexPos2),
        componentsCodec.Seal("Components", (Bond bond) => ((patch_Bond)(object)bond).Components.Where(pair => RegisteredComponents[pair.Key] != null).ToDictionary()).WithDefaut([]),
        (types, pos1, pos2, components) => {
            patch_Bond bond;
            bond = new([.. types.Select(id => BondTypes.GetBondType(id))], pos1, pos2);
            foreach (var component in components) {
                if (bond.HasComponent(component.Key))
                    bond.RemoveComponent(component.Key);
                bond.AddComponent(component.Value);
            }
            return (Bond)(object)bond;
        }
    );

    #endregion

    #region Ctor
    public static event Action<patch_Bond> OnCreate;
    internal static List<Tuple<Identifier[], Func<patch_Bond, IBondComponent>>> CtorsByID = [];

    public void InitObjectsInCtor() {
        Components = [];
    }
    public void AfterCreate() {
        foreach (var ctor in CtorsByID)
            if (ctor.Item1.Any(id => bondTypes.Any(bondT => bondT.Id == id)))
                AddComponent(ctor.Item2(this));
        OnCreate?.Invoke(this);
    }

    // Handled in #region CustomBonds
    //[MonoModILInject(".ctor")]
    //static void PatchCtor(MethodDefinition method, CustomAttribute attribute) {
    //}

    #endregion
}
