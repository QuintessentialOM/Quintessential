#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential.Internal;
using System.Linq;

public class patch_Molecule : Molecule{

    public Maybe<LocString> GetDisplayName() {
        if (displayName.HasValue()) return displayName.GetValue();

        if (GetAtoms().Count == 1) return GetAtoms().First().Value.atomType.elementalName;
        if (GetAtoms().Count == 2 && GetBonds().Count == 1 && GetBonds()[0].type == BondTypeEnum.Standard) {
            bool hasSalt = false;
            bool hasQuicksilver = false;
            LocString otherName = LocString.emptyString;
            foreach (var atom in GetAtoms()) {
                if (atom.Value.atomType == AtomTypes.salt) hasSalt = true;
                else if (atom.Value.atomType == AtomTypes.quicksilver) hasQuicksilver = true;
                else otherName = atom.Value.atomType.name;
            }

            if (hasSalt && hasQuicksilver) return QuintessentialCore.Instance.Translate("naming.cinnabar");
            if (hasSalt && otherName != LocString.emptyString) {
                return patch_LocString.Format(QuintessentialCore.Instance.Translate("naming.stabilized"), otherName);
            }
            if (hasQuicksilver && otherName != LocString.emptyString) {
                return patch_LocString.Format(QuintessentialCore.Instance.Translate("naming.reactive"), otherName);
            }
            if (hasSalt) {
                return Translations.Translate("Bistabilized Salt");
            }
        }
        if (GetAtoms().Count == 4 && GetBonds().Count == 3 && GetBonds()[0].type == BondTypeEnum.Standard && GetBonds()[1].type == BondTypeEnum.Standard && GetBonds()[2].type == BondTypeEnum.Standard) {
            var atoms = GetAtoms();
            if (atoms.TryGetValue(new(0, 0), out var atom) && atom.atomType == AtomTypes.water &&
                atoms.TryGetValue(new(-1, 0), out atom) && atom.atomType == AtomTypes.salt &&
                atoms.TryGetValue(new(1, -1), out atom) && atom.atomType == AtomTypes.salt &&
                atoms.TryGetValue(new(0, 1), out atom) && atom.atomType == AtomTypes.fire
                ) return Translations.Translate("Distilled Alcohol");
        }
        return MaybeHelper.empty;
    }
}