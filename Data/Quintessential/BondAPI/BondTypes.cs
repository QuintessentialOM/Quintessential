using Quintessential;
using System.Collections.Generic;

public static class BondTypes {

    private static List<Identifier> bondIds = [];
    private static Dictionary<Identifier, BondType> bondTypes = [];

    public static void RegisterBondType(BondType bondType, bool isUnbondable = true) {
        if (bondTypes.ContainsKey(bondType.Id)) {
            throw new OpusMagnumException("BondId ( " + bondType.Id + " ) was attempted to be registered multiple times!");
        }
        if (bondType.Id == "#:replaceID") throw new OpusMagnumException("Bond id #:replaceID is reserved for internal use.");
        if (bondIds.Count < 9) throw new OpusMagnumException("Can't add bondTypes before init.");

        bondTypes.Add(bondType.Id, bondType);

        if (isUnbondable) ((patch_BonderInfo)(object)PartTypes.unbonder.bonders[0]).bondTypes.Add(bondType);

        if (bondIds.Count == 9) {
            for (int i = 3; i < 8; i++) {
                if (bondIds[i] == "#:replaceID") {
                    bondIds[i] = bondType.Id;
                    return;
                }
            }
        }
        bondIds.Add(bondType.Id);
    }

    public static IReadOnlyDictionary<Identifier, BondType> GetTypes() {
        return bondTypes;
    }
    public static int GetBondIndex(Identifier id) {
        return bondIds.IndexOf(id);
    }
    public static Identifier GetBondId(int index) {
        return bondIds[index];
    }
    public static BondType GetBondType(Identifier id) {
        return bondTypes[id];
    }
    public static BondType GetBondType(int index) {
        return bondTypes[bondIds[index]];
    }


    public static void InitBonds() {
        bondTypes.Add("om:none", null);
        bondTypes.Add("om:standard", new BondType(
            "om:standard",
            BondTextures.standard,
            Assets.textures.bonds.unbond,
            []
        ));
        bondTypes.Add("om:prisma0", new BondType(
            "om:prisma0",
            BondTextures.prisma0,
            Assets.textures.bonds.unbond_tri,
            ["om:prisma1", "om:prisma2"],
            -10
        ));
        bondTypes.Add("om:prisma1", new BondType(
            "om:prisma1",
            BondTextures.prisma1,
            Assets.textures.bonds.unbond_tri,
            ["om:prisma0", "om:prisma2"]
        ));
        bondTypes.Add("om:prisma2", new BondType(
            "om:prisma2",
            BondTextures.prisma2,
            Assets.textures.bonds.unbond_tri,
            ["om:prisma0", "om:prisma1"],
            10
        ));
        bondIds.Add("om:none");
        bondIds.Add("om:standard");
        bondIds.Add("om:prisma0");
        bondIds.Add("#:replaceID");
        bondIds.Add("om:prisma1");
        bondIds.Add("#:replaceID");
        bondIds.Add("#:replaceID");
        bondIds.Add("#:replaceID");
        bondIds.Add("om:prisma2");
    }
}
