using Quintessential;
using System.Collections.Generic;

// Purposefully declared without a namespace
public class BondType {

    public readonly Identifier Id;

    /// <summary>
    /// Decides the reder order of <see cref="BondType"/>s, lower renderPriority bonds render later.
    /// </summary>
    public readonly int renderPriority;

    public readonly BondOverlapMode overlapMode;

    public BondTexture bondTexture;

    public Texture[] unbondAnim;

    public HashSet<Identifier> overlapIds; // TODO add tag support!

    public HashSet<HexIndex> validDistances;

    /// <param name="id">A unique identifier for the <see cref="BondType"/>.</param>
    /// <param name="textures">The textures for this type.</param>
    /// <param name="unbondAnim">The unbonding animation for this type.</param>
    /// <param name="overlapIds">The identifiers that this bond can overlap with based on <paramref name="overlapMode"/>.</param>
    /// <param name="renderPriority">The render priotity deciding bond render order, lower <paramref name="renderPriority"/> bonds render later.</param>
    /// <param name="overlapMode">The way this bond handles overlap with others.</param>
    /// <param name="validDistances">These are the valid positions a bond can connect to from the (0, 0) position.<br/>The rotated counterparts also get added.</param>
    public BondType(Identifier id, BondTexture textures, Texture[] unbondAnim, HashSet<Identifier> overlapIds, int renderPriority = 0, BondOverlapMode overlapMode = BondOverlapMode.OnlyListed, HexIndex[] validDistances = null) {
        Id = id;
        bondTexture = textures;
        this.overlapIds = overlapIds;
        this.renderPriority = renderPriority;
        this.overlapMode = overlapMode;
        this.unbondAnim = unbondAnim;

        validDistances ??= [new HexIndex(1, 0)];
        HashSet<HexIndex> set = [];
        foreach (var pos in validDistances) {
            for (int i = 0; i < 6; i++) {
                set.Add(pos.Rotated(new HexRotation(i)));
            }
        }
        this.validDistances = set;
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
    /// <summary>Allows listed bonds to overlap.</summary>
    OnlyListed,
    /// <summary>Disallows listed bonds to overlap.</summary>
    AllExcept,
}