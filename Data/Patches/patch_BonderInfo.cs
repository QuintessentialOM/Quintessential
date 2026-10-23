using MonoMod;
using Quintessential;
using System.Collections.Generic;
using System.Linq;

[MonoModPatch("BonderInfo")]
class patch_BonderInfo {

    public List<BondType> bondTypes;
    public List<AtomType> uniqueAtoms;
    public AtomTag uniqueTag;
    public bool isUnbond = false;

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, HexIndex hexPos1, HexIndex hexPos2) {
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        uniqueAtoms = [];
        uniqueTag = null;
    }

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, List<AtomType> uniqueAtoms, HexIndex hexPos1, HexIndex hexPos2) {
        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        this.uniqueAtoms = uniqueAtoms;
        uniqueTag = null;
    }

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, List<AtomType> uniqueAtoms, AtomTag uniqueAtomTag, HexIndex hexPos1, HexIndex hexPos2) {

        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        this.uniqueAtoms = uniqueAtoms;
        uniqueTag = uniqueAtomTag;
    }

    [MonoModConstructor]
    public patch_BonderInfo(List<BondType> bondTypes, bool isUnbond, AtomTag uniqueAtomTag, HexIndex hexPos1, HexIndex hexPos2) {

        this.hexPos1 = hexPos1;
        this.hexPos2 = hexPos2;
        this.bondTypes = bondTypes;
        this.isUnbond = isUnbond;
        uniqueAtoms = [];
        uniqueTag = uniqueAtomTag;
    }

    public virtual bool IsBondable(AtomType atomType) {
        if (uniqueAtoms.Count == 0 && uniqueTag == null) return true;
        if (uniqueAtoms.Count != 0 &&
            uniqueAtoms.Any(atom => atom == atomType)) return true;
        if (uniqueTag != null &&
            uniqueTag.HasAtom(atomType)) return true;
        return false;
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
