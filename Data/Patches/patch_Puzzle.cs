using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

public class patch_Puzzle : ISerializableComponentHolder<patch_Puzzle, IPuzzleComponent> {

    #region ComponentSystem
    private Dictionary<Identifier, IPuzzleComponent> Components;

    public void AddComponent(IPuzzleComponent toAdd) {
        if (!RegisteredComponents.ContainsKey(toAdd.Id))
            RegisterComponent(toAdd.Id, null);
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Puzzle.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<IPuzzleComponent> ctor) {
        if (!RegisteredComponents.ContainsKey(id))
            RegisterComponent(id, null);
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
    public ReadOnlyDictionary<Identifier, IPuzzleComponent> GetComponents() {
        return new ReadOnlyDictionary<Identifier, IPuzzleComponent>(Components);
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

    private static readonly Codec<Tuple<Texture, Texture>> vialTexture = Codec<Tuple<Texture, Texture>>.Create(
        Codecs.STRING.Seal("Primary", (Tuple<Texture, Texture> textures) => patch_DataLoader.GetTexturePath(textures.Item1, "")).WithDefault(""),
        Codecs.STRING.Seal("Secondary", (Tuple<Texture, Texture> textures) => patch_DataLoader.GetTexturePath(textures.Item2, "")).WithDefault(""),
        (primary, secondary) => new Tuple<Texture, Texture>(patch_DataLoader.GetVial(primary), patch_DataLoader.GetVial(secondary))
    );
    private static readonly Codec<List<Tuple<Texture, Texture>>> vialTextures = ListCodec<Tuple<Texture, Texture>>.Create(vialTexture);
    private static readonly Codec<PlacedVial> vialCodec = Codec<PlacedVial>.Create(
        Codecs.HEXINDEX.Seal("Pos", (PlacedVial vial) => vial.hexPos),
        Codecs.BOOL.Seal("TopConnected", (PlacedVial vial) => vial.isTopConnected),
        vialTextures.Seal("Textures", (PlacedVial vial) => [..vial.textures]),
        (pos, topConnected, textures) => new PlacedVial(pos.Q, pos.R, topConnected, [..textures])
    );
    private static readonly Codec<List<PlacedVial>> vialsCodec = ListCodec<PlacedVial>.Create(vialCodec);
    private static readonly Codec<ProductionInfo> productionInfoCodec = Codec<ProductionInfo>.Create(
        Codecs.BOOL.Seal("TightLeft", (ProductionInfo prodInfo) => prodInfo.tightLeftBound).WithDefault(false),
        Codecs.BOOL.Seal("TightRight", (ProductionInfo prodInfo) => prodInfo.tightRightBound).WithDefault(false),
        Codecs.BOOL.Seal("Isolation", (ProductionInfo prodInfo) => prodInfo.requireIsolation).WithDefault(false),
        chambersCodec.Seal("Chambers", (ProductionInfo prodInfo) => [.. prodInfo.chambers]).WithDefault([]),
        conduitsCodec.Seal("Conduits", (ProductionInfo prodInfo) => [.. prodInfo.conduits]).WithDefault([]),
        vialsCodec.Seal("Vial", (ProductionInfo prodInfo) => [.. prodInfo.vials]).WithDefault([]),
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
        Codecs.STRING.Seal("Author", (Puzzle puzzle) => puzzle.journalAuthor.HasValue() ? puzzle.journalAuthor.GetValue() : "").WithDefault(""),
        Codecs.PERMISSIONS.Seal("Permissions", (Puzzle puzzle) => puzzle.permissionFlags),
        Codecs.LIST_ID.Seal("CustomPermissions", (Puzzle puzzle) => puzzle.CustomPermissions != null ? [.. puzzle.CustomPermissions] : []).WithDefault([]),
        moleculesCodec.Seal("Inputs", (Puzzle puzzle) => [.. puzzle.inputs.Select(io => (patch_Molecule)(object)io.molecule)]),
        moleculesCodec.Seal("Outputs", (Puzzle puzzle) => [.. puzzle.outputs.Select(io => (patch_Molecule)(object)io.molecule)]),
        Codecs.INT.Seal("OutputMultiplier", (Puzzle puzzle) => puzzle.outputMultiplier).WithDefault(1),
        productionInfoCodec.Seal("ProductionInfo", (Puzzle puzzle) => puzzle.productionInfo.HasValue() ? puzzle.productionInfo.GetValue() : null).WithDefault(null),
        componentsCodec.Seal("Components", (Puzzle puzzle) => ((patch_Puzzle)(object)puzzle).Components.Where(pair => RegisteredComponents[pair.Key] != null).ToDictionary()).WithDefault([]),
        (id, name, author, permissions, customPermissions, inputs, outputs, multiplier, prodInfo, components) => {
            Puzzle puzzle = new() {
                puzzleId = id,
                puzzleName = name,
                journalAuthor = author,
                permissionFlags = permissions,
                CustomPermissions = customPermissions is null ? null : [.. customPermissions],
                inputs = [.. inputs.Select(io => new PuzzleInputOutput((Molecule)(object)io))],
                outputs = [.. outputs.Select(io => new PuzzleInputOutput((Molecule)(object)io))],
                outputMultiplier = multiplier,
                productionInfo = prodInfo != null ? MaybeHelper.Create(prodInfo) : MaybeHelper.empty,
            };
            foreach (var component in components) {
                if (((patch_Puzzle)(object)puzzle).HasComponent(component.Key))
                    ((patch_Puzzle)(object)puzzle).RemoveComponent(component.Key);
                ((patch_Puzzle)(object)puzzle).AddComponent(component.Value);
            }
            puzzle.CalculateCabinetBounds();
            return puzzle;
        }
    );
    [MonoModIgnore] // # unsafe assuming first patch
    public extern void layer_0_SaveToFile(string path);
    public void SaveToFile(string path) {
        bool enabledVanillaFormat = true;

        var encoded = PUZZLE.Encode(JsonCodecMap.Instance, (Puzzle)(object)this);
        bool isVanillaSafe = false;

        if (enabledVanillaFormat && path == GetFilePath()) {
            try {
                Puzzle s = (Puzzle)(object)this;
                if (s.inputs.All(i => i.molecule.GetAtoms().All(atom => atom.Value.atomType.Id.namespc.Equals("om"))) &&
                    s.outputs.All(i => i.molecule.GetAtoms().All(atom => atom.Value.atomType.Id.namespc.Equals("om"))))
                {
                    if (!Directory.Exists("modded/temp")) {
                        Directory.CreateDirectory("modded/temp");
                    }
                    string tempPath = Path.Combine("modded/temp", s.puzzleId + ".puzzle");
                    layer_0_SaveToFile(tempPath);
                    var transformed = Puzzle.LoadFromFile(tempPath);
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    if (transformed.puzzleId == s.puzzleId)
                        isVanillaSafe = encoded.ToJsonString() == PUZZLE.Encode(JsonCodecMap.Instance, transformed).ToJsonString();
                    //Logger.LogNoTime("#------");
                    //Logger.LogNoTime(encoded.ToJsonString());
                    //Logger.LogNoTime(PUZZLE.Encode(JsonCodecMap.Instance, transformed).ToJsonString());
                }
            } catch { }
        }

        if (isVanillaSafe) {
            ChangeFileExtension("", ref path, () => layer_0_SaveToFile(path));
        } else if (ExtraFileExtension == "") {
            ChangeFileExtension(".json", ref path, () => File.WriteAllText(path, encoded.ToJsonString()));
        } else if (ExtraFileExtension == ".json") {
            File.WriteAllText(path, encoded.ToJsonString());
        } else if (ExtraFileExtension == ".jsonc") { // Human readable version of
            File.WriteAllText(path, encoded.ToString());
        } else
            throw new Exception("Invalid extra file extension '" + ExtraFileExtension + "' for puzzle.");
    }
    private void ChangeFileExtension(string newExtension, ref string path, Action save) {
        if (newExtension != ExtraFileExtension) {

            bool wasVanillaFormat = Path.GetExtension(path) == ".puzzle";
            string origPathStripped = Path.Combine(Path.GetDirectoryName(path), wasVanillaFormat ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path));
            newExtension ??= "";
            string orig = path;
            path = origPathStripped + newExtension;
            ExtraFileExtension = newExtension;
            if (save != null) {
                save();
                if (File.Exists(orig) && orig != path) {
                    File.Delete(orig);
                }
            }
        } else
            save?.Invoke();
    }
    private string GetFilePath() {
        return GameLogic.instance.workshopManager.CustomPuzzlePath((Puzzle)(object)this);
    }

    [MonoModIgnore] public static extern Puzzle orig_LoadFromFile(string path);
    public static Puzzle LoadFromFile(string path) {
        if (Path.GetExtension(path) == ".json" || Path.GetExtension(path) == ".jsonc") {
            string file = File.ReadAllText(path);
            if (Path.GetExtension(path) == ".jsonc") file = DataSerializer.JsoncToJson(file);

            Puzzle p = PUZZLE.Decode(JsonCodecMap.Instance,JsonNode.Parse(file));
            ((patch_Puzzle)(object)p).ExtraFileExtension = Path.GetExtension(path);
            return p;
        }
        
        var pOrig = orig_LoadFromFile(path);
        ((patch_Puzzle)(object)pOrig).ExtraFileExtension = "";
        return pOrig; ;
    }
    public string ExtraFileExtension;

    [MonoModILInject("WriteMoleculeToFile")]
    static void WriteBondToFile(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference getBytes = MonoModRule.Modder.FindType("Bond").Resolve().Methods.First(f => f.Name.Equals("GetVanillaTypeData"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Bond", "type"));
        cursor.Remove();
        cursor.EmitCallvirt(getBytes);
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
