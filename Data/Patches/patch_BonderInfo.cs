using MonoMod;
using System.Collections.Generic;

[MonoModPatch("BonderInfo")]
class patch_BonderInfo {

    public List<BondType> bondTypes;
    public List<AtomType> uniqueAtoms;
    public bool isUnbond = false;

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, HexIndex hexPos1, HexIndex hexPos2) {
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        uniqueAtoms = [];
    }

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, List<AtomType> uniqueAtoms, HexIndex hexPos1, HexIndex hexPos2) {
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        this.uniqueAtoms = uniqueAtoms;
    }

    [MonoModRemove] [MonoModConstructor]
    public patch_BonderInfo(HexIndex hexPos1, HexIndex hexPos2, BondTypeEnum bondType, Maybe<AtomType> uniqueAtom) { }

    public readonly HexIndex hexPos1;
    public readonly HexIndex hexPos2;
    [MonoModRemove]
    public readonly BondTypeEnum bondType;
    [MonoModRemove]
    public Maybe<AtomType> uniqueAtom;
}
