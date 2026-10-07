using Quintessential;
using System.Collections.Generic;

// Purposefully declared without a namespace
public class BondType {

    public readonly Identifier Id;

    /// <summary> Lower renderPriority bonds render later. </summary>
    public readonly int renderPriority;

    public readonly BondOverlapMode overlapMode;

    public BondTexture bondTexture;

    public Texture[] unbondAnim;

    public HashSet<Identifier> overlapIds;

    public BondType(Identifier id, BondTexture textures, Texture[] _unbondAnim, HashSet<Identifier> _overlapIds, int _renderPriority = 0, BondOverlapMode _overlapMode = BondOverlapMode.OnlyListed) {
        Id = id;
        bondTexture = textures;
        overlapIds = _overlapIds;
        renderPriority = _renderPriority;
        overlapMode = _overlapMode;
        unbondAnim = _unbondAnim;
    }

    /// <returns> True if the bond allows the other, or the other allows this. </returns>
    public bool CanOverlapBond(BondType bond) {
        return (overlapIds.Contains(bond.Id) != (overlapMode == BondOverlapMode.AllExcept)) ||
               (bond.overlapIds.Contains(Id) != (bond.overlapMode == BondOverlapMode.AllExcept));
    }

    public static implicit operator BondType(Identifier id) {
        return BondTypes.GetBondType(id);
    }
    public static implicit operator BondType(string id) {
        return BondTypes.GetBondType(id);
    }
}

/// <summary> OnlyListed allows listed bonds, AllExcept allows all unlisted bonds to overlap </summary>
public enum BondOverlapMode {
    OnlyListed,
    AllExcept,
}