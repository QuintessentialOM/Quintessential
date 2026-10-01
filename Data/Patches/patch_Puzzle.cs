using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

public class patch_Puzzle : ISerializableComponentHolder<patch_Puzzle, IPuzzleComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, IPuzzleComponent> Components;

    public void AddComponent(IPuzzleComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Puzzle.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IPuzzleComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            throw new Exception("Attempted to add a component that wasnt Registered.\nTry to register the component type with RegisterComponent() first.");
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out IPuzzleComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public IPuzzleComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out IPuzzleComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    private static Dictionary<Identifier, Codec<IPuzzleComponent>> RegisteredComponents = [];
    public static void RegisterComponent(Identifier Id, Codec<IPuzzleComponent> codec) {
        RegisteredComponents[Id] = codec;
    }

    #endregion

    #region ComponentCalls
    #endregion

    #region Serialization
    private static readonly Codec<Dictionary<Identifier, IPuzzleComponent>> componentsCodec = CatalogueCodec<Identifier, IPuzzleComponent>.Create(
        RegisteredComponents, id => id.ToString(), str => new Identifier(str)
    );
    private static readonly Codec<List<patch_Molecule>> moleculesCodec = ListCodec<patch_Molecule>.Create(Codecs.MOLECULE);
    private static readonly Codec<PlacedChamber> chamberCodec = Codec<PlacedChamber>.Create(
        Codecs.HEXINDEX.Seal("Pos", (PlacedChamber chamber) => chamber.hexPos),
        Codecs.STRING.Seal("Name", (PlacedChamber chamber) => chamber.chamber.name),
        (pos, name) => {
            var chamber = Puzzles.prodChambers.First(chamber => chamber.name == name);
            return new PlacedChamber(pos.Q, pos.R, chamber);
        }
    );
    private static readonly Codec<List<PlacedChamber>> chambersCodec = ListCodec<PlacedChamber>.Create(chamberCodec);
    private static readonly Codec<PlacedConduit> conduitCodec = Codec<PlacedConduit>.Create(
        Codecs.HEXINDEX.Seal("Pos1", (PlacedConduit conduit) => conduit.conduitTransforms[0].translation),
        Codecs.HEXINDEX.Seal("Pos2", (PlacedConduit conduit) => conduit.conduitTransforms[1].translation),
        Codecs.LIST_HEXINDEX.Seal("Hexes", (PlacedConduit conduit) => [.. conduit.conduitHexes]),
        (pos1, pos2, hexes) =>  new PlacedConduit(pos1.Q, pos1.R, pos2.Q, pos2.R, [.. hexes])
    );
    private static readonly Codec<List<PlacedConduit>> conduitsCodec = ListCodec<PlacedConduit>.Create(conduitCodec);
    private static readonly Codec<PlacedVial> vialCodec = Codec<PlacedVial>.Create(
        Codecs.HEXINDEX.Seal("Pos", (PlacedVial vial) => vial.hexPos),
        Codecs.BOOL.Seal("TopConnected", (PlacedVial vial) => vial.isTopConnected),
        Codecs.INT.Seal("TextureCount", (PlacedVial vial) => vial.textures.Length),
        (pos, topConnected, textureCount) => {
            Tuple<Texture, Texture>[] textures = new Tuple<Texture, Texture>[textureCount];
            for (int i = 0; i < textureCount; i++)
                textures[i] = Tuple.Create(Assets.textures.pipelines.vials.generic.empty, Assets.textures.pipelines.vials.generic.empty);
            return new PlacedVial(pos.Q, pos.R, topConnected, textures);
        }
    );
    private static readonly Codec<List<PlacedVial>> vialsCodec = ListCodec<PlacedVial>.Create(vialCodec);
    private static readonly Codec<ProductionInfo> productionInfoCodec = Codec<ProductionInfo>.Create(
        Codecs.BOOL.Seal("TightLeft", (ProductionInfo prodInfo) => prodInfo.tightLeftBound).WithDefaut(false),
        Codecs.BOOL.Seal("TightRight", (ProductionInfo prodInfo) => prodInfo.tightRightBound).WithDefaut(false),
        Codecs.BOOL.Seal("Isolation", (ProductionInfo prodInfo) => prodInfo.requireIsolation).WithDefaut(false),
        chambersCodec.Seal("Chambers", (ProductionInfo prodInfo) => [.. prodInfo.chambers]).WithDefaut([]),
        conduitsCodec.Seal("Conduits", (ProductionInfo prodInfo) => [.. prodInfo.conduits]).WithDefaut([]),
        vialsCodec.Seal("Vial", (ProductionInfo prodInfo) => [.. prodInfo.vials]).WithDefaut([]),
        (tightLeft, tightRight, isolation, chambers, conduits, vials) => {
            return new() {
                tightLeftBound = tightLeft,
                tightRightBound = tightRight,
                requireIsolation = isolation,
                chambers = [.. chambers],
                conduits = [.. conduits],
                vials = [.. vials]
            };
        }
    );
    internal static readonly Codec<Puzzle> PUZZLE = Codec<Puzzle>.Create(
        Codecs.STRING.Seal("Id", (Puzzle puzzle) => puzzle.puzzleId),
        Codecs.LOCSTRING.Seal("Name", (Puzzle puzzle) => puzzle.puzzleName),
        Codecs.STRING.Seal("Author", (Puzzle puzzle) => puzzle.journalAuthor.HasValue() ? puzzle.journalAuthor.GetValue() : "").WithDefaut(""),
        Codecs.PERMISSIONS.Seal("Permissions", (Puzzle puzzle) => puzzle.permissionFlags),
        Codecs.LIST_ID.Seal("CustomPermissions", (Puzzle puzzle) => puzzle.CustomPermissions != null ? [.. puzzle.CustomPermissions] : []).WithDefaut([]),
        moleculesCodec.Seal("Inputs", (Puzzle puzzle) => [.. puzzle.inputs.Select(io => (patch_Molecule)(object)io.molecule)]),
        moleculesCodec.Seal("Outputs", (Puzzle puzzle) => [.. puzzle.outputs.Select(io => (patch_Molecule)(object)io.molecule)]),
        Codecs.INT.Seal("OutputMultiplier", (Puzzle puzzle) => puzzle.outputMultiplier).WithDefaut(1),
        productionInfoCodec.Seal("ProductionInfo", (Puzzle puzzle) => puzzle.productionInfo.HasValue() ? puzzle.productionInfo.GetValue() : null).WithDefaut(null),
        componentsCodec.Seal("Components", (Puzzle puzzle) => ((patch_Puzzle)(object)puzzle).Components).WithDefaut([]),
        (id, name, author, permissions, customPermissions, inputs, outputs, multiplier, prodInfo, components) => {
            Puzzle puzzle = new() {
                puzzleId = id,
                puzzleName = name,
                journalAuthor = author,
                permissionFlags = permissions,
                CustomPermissions = [.. customPermissions],
                inputs = [.. inputs.Select(io => new PuzzleInputOutput((Molecule)(object)io))],
                outputs = [.. outputs.Select(io => new PuzzleInputOutput((Molecule)(object)io))],
                outputMultiplier = multiplier,
                productionInfo = prodInfo != null ? MaybeHelper.Create(prodInfo) : MaybeHelper.empty,
            };
            ((patch_Puzzle)(object)puzzle).Components = components;
            return puzzle;
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
