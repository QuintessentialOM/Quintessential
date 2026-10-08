using System;
using System.Linq;

namespace Quintessential.Internal;

internal static class VanillaRecipes {

    public static bool BasicCalcification(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var calcified) ||
            !AtomTag.AtomTags["om:calcifiable"].HasAtom(calcified)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)calcified);
        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.salt);
        return true;
    }
    public static bool BasicDuplication(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), true, out var duplicated) ||
            !AtomTag.AtomTags["om:duplicatable"].HasAtom(duplicated) ||
            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var overwritten) ||
            overwritten.atomType != AtomTypes.salt) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)duplicated);
        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)overwritten);
        return true;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public static bool BasicProjection(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(1, 0), false, out var projected) ||
            !AtomTag.AtomTags["om:$successor_proj"].HasAtom(projected, out var result) ||
            !(7 < projected.atomType.byteId && projected.atomType.byteId <= 12) || // Check if atom is an OM metal
            !sim.GetAtomReference(part, new HexIndex(0, 0), false, out var consumed) ||
            consumed.isHeldByArm || consumed.inMultiAtomMolecule || consumed.atomType != AtomTypes.quicksilver) return false;

        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)projected);
        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)consumed);
        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.GetByID(result.Value));
        return true;
    }
    public static bool BasicRejection(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var rejected) ||
            !AtomTag.AtomTags["om:$predecessor"].HasAtom(rejected, out var result) ||
            !(7 <= rejected.atomType.byteId && rejected.atomType.byteId < 12) || // Check if atom is an OM metal
            sim.HasAtomAt(part, new HexIndex(1, 0), false)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)rejected);
        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.GetByID(result.Value));
        sim.RecipeOutputs.Add(new HexIndex(1, 0), (patch_AtomType)(object)AtomTypes.quicksilver);
        return true;
    }
#pragma warning restore CS0618 // Type or member is obsolete
    public static bool BasicPurification(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var purified0) ||
            purified0.inMultiAtomMolecule || purified0.isHeldByArm ||
            !AtomTag.AtomTags["om:$successor_purif"].HasAtom(purified0, out var result) ||
            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var purified1) ||
            purified1.inMultiAtomMolecule || purified1.isHeldByArm ||
            purified0.atomType != purified1.atomType ||
            sim.HasAtomAt(part, new HexIndex(0, 1), false)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)purified0);
        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)purified1);
        sim.RecipeOutputs.Add(new HexIndex(0, 1), (patch_AtomType)(object)AtomTypes.GetByID(result.Value));
        return true;
    }
    public static bool BasicDivision(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var dividend) ||
            dividend.inMultiAtomMolecule || dividend.isHeldByArm ||
            !AtomTag.AtomTags["om:$division_upper"].HasAtom(dividend, out var upper) ||
            !AtomTag.AtomTags["om:$division_lower"].HasAtom(dividend, out var lower) ||
            sim.HasAtomAt(part, new HexIndex(-1, 0), false) ||
            sim.HasAtomAt(part, new HexIndex(1, 0), false)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)dividend);
        sim.RecipeOutputs.Add(new HexIndex(-1, 0), (patch_AtomType)(object)AtomTypes.GetByID(upper.Value));
        sim.RecipeOutputs.Add(new HexIndex(1, 0), (patch_AtomType)(object)AtomTypes.GetByID(lower.Value));
        return true;
    }
    public static bool BasicProliferation(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(-1, 1), true, out var prolified) ||
            !AtomTag.AtomTags["om:proliferatable"].HasAtom(prolified) ||
            !sim.GetAtomReference(part, new HexIndex(1, 1), false, out var consumed) ||
            sim.HasAtomAt(part, new HexIndex(1, -1), false) ||
            consumed.atomType != AtomTypes.quicksilver ||
            consumed.inMultiAtomMolecule || consumed.isHeldByArm) return false;

        sim.RecipeInputs.Add(new HexIndex(-1, 1), (patch_AtomReference)(object)prolified);
        sim.RecipeInputs.Add(new HexIndex(1, 1), (patch_AtomReference)(object)consumed);
        return true;
    }

    public static bool BasicAnimismus(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var consumed0) ||
            consumed0.atomType != AtomTypes.salt ||
            consumed0.inMultiAtomMolecule || consumed0.isHeldByArm ||
            !sim.GetAtomReference(part, new HexIndex(1, 0), false, out var consumed1) ||
            consumed1.atomType != AtomTypes.salt ||
            consumed1.inMultiAtomMolecule || consumed1.isHeldByArm ||
            sim.HasAtomAt(part, new HexIndex(0, 1), false) ||
            sim.HasAtomAt(part, new HexIndex(1, -1), false)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)consumed0);
        sim.RecipeInputs.Add(new HexIndex(1, 0), (patch_AtomReference)(object)consumed1);
        sim.RecipeOutputs.Add(new HexIndex(0, 1), (patch_AtomType)(object)AtomTypes.vitae);
        sim.RecipeOutputs.Add(new HexIndex(1, -1), (patch_AtomType)(object)AtomTypes.mors);
        return true;
    }

    public static bool BasicUnification(patch_Sim sim, Part part) {
        var cardinals = new AtomReference[4];
        if (!sim.GetAtomReference(part, new HexIndex(-1, 1), false, out cardinals[0]) ||
            cardinals[0].inMultiAtomMolecule || cardinals[0].isHeldByArm ||
            !sim.GetAtomReference(part, new HexIndex(0, 1), false, out cardinals[1]) ||
            cardinals[1].inMultiAtomMolecule || cardinals[1].isHeldByArm ||
            !sim.GetAtomReference(part, new HexIndex(0, -1), false, out cardinals[2]) ||
            cardinals[2].inMultiAtomMolecule || cardinals[2].isHeldByArm ||
            !sim.GetAtomReference(part, new HexIndex(1, -1), false, out cardinals[3]) ||
            cardinals[3].inMultiAtomMolecule || cardinals[3].isHeldByArm ||
            sim.HasAtomAt(part, new HexIndex(0, 0), false) ||
            cardinals.Count(atom => atom.atomType == AtomTypes.fire) != 1 ||
            cardinals.Count(atom => atom.atomType == AtomTypes.water) != 1 ||
            cardinals.Count(atom => atom.atomType == AtomTypes.earth) != 1 ||
            cardinals.Count(atom => atom.atomType == AtomTypes.air) != 1) return false;

        sim.RecipeInputs.Add(new HexIndex(-1, 1), (patch_AtomReference)(object)cardinals[0]);
        sim.RecipeInputs.Add(new HexIndex(0, 1), (patch_AtomReference)(object)cardinals[1]);
        sim.RecipeInputs.Add(new HexIndex(0, -1), (patch_AtomReference)(object)cardinals[2]);
        sim.RecipeInputs.Add(new HexIndex(1, -1), (patch_AtomReference)(object)cardinals[3]);
        sim.RecipeOutputs.Add(new HexIndex(0, 0), (patch_AtomType)(object)AtomTypes.quintessence);
        return true;
    }
    public static bool BasicDispersion(patch_Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new HexIndex(0, 0), false, out var prismatic) ||
            prismatic.inMultiAtomMolecule || prismatic.isHeldByArm ||
            prismatic.atomType != AtomTypes.quintessence ||
            sim.HasAtomAt(part, new HexIndex(-1, 0), false) ||
            sim.HasAtomAt(part, new HexIndex(0, -1), false) ||
            sim.HasAtomAt(part, new HexIndex(1, -1), false) ||
            sim.HasAtomAt(part, new HexIndex(1, 0), false)) return false;

        sim.RecipeInputs.Add(new HexIndex(0, 0), (patch_AtomReference)(object)prismatic);
        sim.RecipeOutputs.Add(new HexIndex(-1, 0), (patch_AtomType)(object)AtomTypes.air);
        sim.RecipeOutputs.Add(new HexIndex(0, -1), (patch_AtomType)(object)AtomTypes.fire);
        sim.RecipeOutputs.Add(new HexIndex(1, -1), (patch_AtomType)(object)AtomTypes.water);
        sim.RecipeOutputs.Add(new HexIndex(1, 0), (patch_AtomType)(object)AtomTypes.earth);
        return true;
    }
}