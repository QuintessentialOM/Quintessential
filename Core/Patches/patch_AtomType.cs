using MonoMod;
using Quintessential;
using System;

class patch_AtomType {
    [Obsolete("This shouldn't be used. Use `Id` instead.")]
    public byte byteId;
    [Obsolete("This field is never used, it's only here for legacy compat reasons.")]
    public bool isQuicksilver;
    [Obsolete("This shouldn't be used. Use the `om:$successor_proj/purif` tags instead.")]
    public Maybe<AtomType> successorMetal;
    [Obsolete("This shouldn't be used. Use the `om:$predecessor` tag instead.")]
    public Maybe<AtomType> predecessorMetal;
    [Obsolete("This shouldn't be used. Use the `om:$division_upper/lower` tags instead.")]
    public Maybe<MetalDivision> metalDivision;

    public Identifier Id;

    public override string ToString() {
        return Id;
    }

    [MonoModReplace]
    public override bool Equals(object obj) {
        return obj is patch_AtomType type && this == type;
    }

    [MonoModReplace]
    public override int GetHashCode() {
        return byteId.GetHashCode() ^ Id.GetHashCode();
    }

    [MonoModReplace]
    public static bool operator ==(patch_AtomType atomType1, patch_AtomType atomType2) {
        return (atomType1.byteId == atomType2.byteId) && atomType1.Id == atomType2.Id;
    }

    [MonoModReplace]
    public static bool operator !=(patch_AtomType atomType1, patch_AtomType atomType2) {
        return !(atomType1 == atomType2);
    }
}