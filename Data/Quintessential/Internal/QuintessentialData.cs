using Quintessential.Serialization;

namespace Quintessential.Internal;

internal class QuintessentialData : QuintessentialMod, IDataMod {
    public override string ModId => "quintessential_data";
    public static QuintessentialData Instance { get; }

    public override void Load() {
        DataSerializer.AssignConverter(AtomTagJsonConverter.Get());
        DataSerializer.AssignConverter(PartTagJsonConverter.Get());
    }

    public override void LoadContent() {
        this.AddRecipe(new GlyphRecipe(),"om:bonder", "om:bonder");
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
    public override void LoadCompatContent() {
        //var bm = new CodecsBenchmark();
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(1, false);
        //bm.TestCompareDynamic(2, false);
        //bm.TestCompareDynamic(2, false);
        //new CodecsBenchmark().Test(1000, false);
        //new CodecsBenchmark().Test(10000, false);
        //new CodecsBenchmark().Test(100000, false);
    }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
    public override void Unload() { }
}
