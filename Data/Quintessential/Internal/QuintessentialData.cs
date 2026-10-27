using Quintessential.Components;
using Quintessential.Serialization;
using System.IO;
using System.Linq;
//using Quintessential.Settings;
//using System;
//using System.IO;
//using System.Text.Json.Nodes;
//using YamlDotNet.Serialization;

namespace Quintessential.Internal;

internal class QuintessentialData : QuintessentialMod, IDataMod {
    public override string ModId => "quintessential_data";
    public static QuintessentialData Instance { get; }
    //public override Type SettingsType => typeof(QuintessentialDataSettings);

    public override void Load() {
        DataSerializer.AssignConverter(AtomTagJsonConverter.Get());
        DataSerializer.AssignConverter(PartTagJsonConverter.Get());
    }

    public override void LoadContent() {
        foreach (var bonder in PartTypes.triplexBonder.bonders) {
            ((patch_BonderInfo)(object)bonder).uniqueTag = AtomTag.AtomTags.GetOrInsert("om:triplexable", new AtomTag("om:triplexable"));
        }
        this.AddRecipe(new GlyphRecipe(), "om:bonder", "om:bonder");
        this.AddRecipe(new GlyphRecipe(), "om:unbonder", "om:unbonder");
        this.AddRecipe(new GlyphRecipe(), "om:multi_bonder", "om:multi_bonder");
        this.AddRecipe(new GlyphRecipe(), "om:triplex_bonder", "om:triplex_bonder");
        this.AddRecipe(new GlyphRecipe(), "om:conduit", "om:conduit");
        this.AddRecipe(new GlyphRecipe(), "om:disposal", "om:disposal");

        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicCalcification), "om:calcification", "om:calcification");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicDuplication), "om:duplication", "om:duplication");

        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicProjection), "om:projection", "om:projection");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicRejection), "om:rejection", "om:rejection");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicPurification), "om:purification", "om:purification");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicDivision), "om:division", "om:division");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicProliferation), "om:proliferation", "om:proliferation");

        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicAnimismus), "om:animismus", "om:animismus");

        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicUnification), "om:unification", "om:unification");
        this.AddRecipe(new GlyphRecipe(VanillaRecipes.BasicDispersion), "om:dispersion", "om:dispersion");

        /*
        this.AddRecipe(new GlyphRecipe((sim, part) => {
            if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var calcified) ||
                calcified.atomType != AtomTypes.quintessence) return false;

            var cardinals = new AtomReference[6];
            sim.GetAtomReference(part, new HexIndex(0, 1), false, out cardinals[0]);
            sim.GetAtomReference(part, new HexIndex(1, 0), false, out cardinals[1]);
            sim.GetAtomReference(part, new HexIndex(1, -1), false, out cardinals[2]);
            sim.GetAtomReference(part, new HexIndex(0, -1), false, out cardinals[3]);
            sim.GetAtomReference(part, new HexIndex(-1, 0), false, out cardinals[4]);
            sim.GetAtomReference(part, new HexIndex(-1, 1), false, out cardinals[5]);
            if (!cardinals.Any(atom => atom != null && atom.atomType == AtomTypes.fire) ||
                !cardinals.Any(atom => atom != null && atom.atomType == AtomTypes.water) ||
                !cardinals.Any(atom => atom != null && atom.atomType == AtomTypes.earth) ||
                !cardinals.Any(atom => atom != null && atom.atomType == AtomTypes.air)) return false;

            sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)calcified);
            sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.salt);
            return true;
        }), "example:quint_calcification", "om:calcification");


        this.AddRecipe(new GlyphRecipe((sim, part) => {
            if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var rejected) ||
                rejected.atomType != AtomTypes.mors ||
                sim.HasAtomAt(part, new HexIndex(1, 0), false)) return false;

            bool movingPart = sim.GetPartsAt(part, new HexIndex(0, 1), PartTag.PartTags["om:arm"], out var parts) && parts.Count == 1;
            movingPart = movingPart && (parts[0].Value.deltaPosition == new HexIndex(1, 0).Rotated(part.GetRotation()));
            if (!movingPart) {
                movingPart = sim.GetPartsAt(part, new HexIndex(1, -1), PartTag.PartTags["om:arm"], out parts) && parts.Count == 1;
                movingPart = movingPart && (parts[0].Value.deltaPosition == new HexIndex(1, 0).Rotated(part.GetRotation()));
                if (!movingPart) return false;
            }

            sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)rejected);
            sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.salt);
            sim.RecipeOutputs.Add(new HexIndex(1, 0), (patch_AtomType)(object)AtomTypes.salt);
            return true;
        }), "example:momentum_rejection", "om:rejection");
        */

        /*
        var EtherealTextures = new BondTexture {
            texture = AssetLoaderHelper.LoadTexture(Path.Combine("textures", ModId, "bonds", "golden")),
            normalMap = Assets.textures.black,
            bondAnim = BondTextures.standard.bondAnim,
            glyphAnim = BondTextures.standard.glyphAnim,
            bondSound = BondTextures.standard.bondSound,
            textureOffset = new Vector2(1, 0),
        };
        this.AddBondType(new BondType(
          GetIdentifier("golden"),
          EtherealTextures,
          BondTypes.GetBondType("om:standard").unbondAnim,
          [],
          0,
          BondOverlapMode.OnlyListed,
          [new HexIndex(-1, 2)]
        ), false);
        */

        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:animismus",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var atomReference) ||
        //            atomReference.atomType != AtomTypes.repeat ||
        //            atomReference.inMultiAtomMolecule || atomReference.isHeldByArm ||
        //            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var atomReference2) ||
        //            atomReference2.atomType != AtomTypes.gold ||
        //            atomReference2.inMultiAtomMolecule || atomReference2.isHeldByArm ||
        //            sim.HasAtomAt(part, new HexIndex(0, 1), false) ||
        //            sim.HasAtomAt(part, new HexIndex(1, -1), false)) return false;

        //        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)atomReference2);
        //        sim.RecipeOutputs.Add(new HexIndex(0, 1), (patch_AtomType)(object)AtomTypes.quintessence);
        //        sim.RecipeOutputs.Add(new HexIndex(1, -1), (patch_AtomType)(object)AtomTypes.salt);
        //        return true;
        //    })
        //}, "om:animalius");

        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:proliferation",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(-1, 1), true, out var atomReference) ||
        //            !AtomTag.AtomTags["om:duplicatable"].HasAtom(atomReference) ||
        //            !sim.GetAtomReference(part, new HexIndex(1, 1), false, out var atomReference2) ||
        //            sim.HasAtomAt(part, new HexIndex(1, -1), false) ||
        //            atomReference2.atomType != AtomTypes.salt ||
        //            atomReference2.inMultiAtomMolecule || atomReference2.isHeldByArm) return false;

        //        sim.RecipeInputs.Add(new HexIndex(-1, 1), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeInputs.Add(new HexIndex(1, 1), (patch_AtomReference)(object)atomReference2);
        //        return true;
        //    })
        //}, "om:cardinal_proliferation");

        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:purification",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var atomReference) ||
        //            atomReference.inMultiAtomMolecule || atomReference.isHeldByArm ||
        //            atomReference.atomType != AtomTypes.quintessence ||
        //            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var atomReference2) ||
        //            atomReference2.inMultiAtomMolecule || atomReference2.isHeldByArm ||
        //            atomReference.atomType != atomReference2.atomType ||
        //            sim.HasAtomAt(part, new HexIndex(0, 1), false)) return false;

        //        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)atomReference2);
        //        sim.RecipeOutputs.Add(new HexIndex(0, 1), (patch_AtomType)(object)AtomTypes.repeat);
        //        return true;
        //    })
        //}, "om:pure_prismatic_salt");

        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:rejection",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var atomReference) ||
        //            (atomReference.atomType != AtomTypes.lead && atomReference.atomType != AtomTypes.quintessence) ||
        //            sim.HasAtomAt(part, new HexIndex(1, 0), true)) return false;

        //        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.lead);
        //        sim.RecipeOutputs.Add(new HexIndex(1, 0), (patch_AtomType)(object)AtomTypes.quicksilver);
        //        return true;
        //    })
        //}, "om:infinite_rejection");
        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:projection",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(1, 0), false, out var atomReference) ||
        //            atomReference.atomType != AtomTypes.quintessence ||
        //            !sim.GetAtomReference(part, new HexIndex(0, 0), false, out var atomReference2) ||
        //            atomReference2.isHeldByArm || atomReference2.inMultiAtomMolecule || atomReference2.atomType != AtomTypes.quicksilver) return false;

        //        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)atomReference2);
        //        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.gold);
        //        return true;
        //    })
        //}, "om:quintessential_projection");
        //this.AddRecipe(new GlyphRecipe() {
        //    RecipeGlyphId = "om:duplication",
        //    Predicate = new((sim, part) => {
        //        if (!sim.GetAtomReference(part, new HexIndex(0, 0), true, out var atomReference) ||
        //            atomReference.atomType != AtomTypes.quicksilver ||
        //            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var atomReference2) ||
        //            atomReference2.atomType != AtomTypes.fire) return false;

        //        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)atomReference);
        //        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)atomReference2);
        //        return true;
        //    })
        //}, GetIdentifier("quix_duplication"));
    }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
    public override void Unload() { }
}

//public class QuintessentialDataSettings {

//    [SettingsLabelKey("quintessential_data.settings.run_test")]
//    [YamlIgnore]
//    public SettingsButton DumpAtomSprites = OpenSecretLevel;

//    public static JsonNode Solution;
//    public static JsonNode Puzzle;
//    public static Puzzle Pu;
//    public static Solution Solu;
//    static bool WasInit = false;
//    private static void OpenSecretLevel() {
//        //if (Solution == null || Puzzle == null) return;

//        string puzzlePath = Path.Combine(QuintessentialLoader.PathModSaves, "Quintessential", "test.puzzle.json");
//        string solutionPath = Path.Combine(QuintessentialLoader.PathModSaves, "Quintessential", "test.solution.json");
//        //File.WriteAllText(solutionPath, Solution.ToString());
//        //File.WriteAllText(puzzlePath, Puzzle.ToString());



//        if (!WasInit) {
//            WasInit = true;
//            Pu = Codecs.PUZZLE.Decode(JsonCodecMap.Instance, JsonNode.Parse(File.ReadAllText(puzzlePath)));

//            Array.Resize(ref Puzzles.campaignPuzzles, Puzzles.campaignPuzzles.Length + 1);
//            Puzzles.campaignPuzzles[^1] = Pu;

//            Solu = Codecs.SOLUTION.Decode(JsonCodecMap.Instance, JsonNode.Parse(File.ReadAllText(solutionPath)));
//            GameLogic.instance.globalSolutions.Add(Solu);
//        }

//        //Logger.LogNoTime(p.puzzleId);
//        //Logger.LogNoTime(GameLogic.instance.globalSolutions[^1].GetPuzzle().puzzleId);
//        //p.puzzleId = "TestClone";
//        //GameLogic.instance.globalSolutions[^1].name = "Loaded Puzzle";
//        //((patch_Solution)(object)GameLogic.instance.globalSolutions[^1]).TestSetPuzzle(p);
//        GameLogic.instance.PopScreen();
//        GameLogic.instance.PushScreen(new PuzzleInfoScreen(Pu, Assets.musicTracks.solving8, Assets.sounds.fanfare_solving4, MaybeHelper.empty), MaybeHelper.empty, Transitions.raise);
//    }
//}
