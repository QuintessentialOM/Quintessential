#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential;
using Quintessential.Internal;
using System.Linq;

public class patch_Molecule : Molecule{

    public Maybe<LocString> GetDisplayName() {
        if (displayName.HasValue()) return displayName.GetValue();

        foreach (var item in QApi.MoleculeNames) {
            if (item.Item1?.Invoke(this) ?? false) {
                return item.Item2;
            }   
        }

        if (GetAtoms().Count == 1) return GetAtoms().First().Value.atomType.elementalName;
        if (GetAtoms().Count == 2 && GetBonds().Count == 1 && IsStandardBond(GetBonds()[0])) {
            if (GetAtoms().All(atom => atom.Value.atomType == AtomTypes.salt))
                return Translations.Translate("Bistabilized Salt");

            if (GetAtoms().Any(atom => atom.Value.atomType == AtomTypes.salt) && GetAtoms().Any(atom => atom.Value.atomType == AtomTypes.quicksilver))
                return QuintessentialCore.Instance.Translate("naming.cinnabar");

            foreach (var item in QApi.AtomTypePrefixes) {
                bool hasType = false;
                LocString otherName = LocString.emptyString;
                foreach (var atom in GetAtoms()) {
                    if (atom.Value.atomType == item.Item1) hasType = true;
                    else otherName = atom.Value.atomType.name;
                }
                if (hasType && otherName != LocString.emptyString) {
                    return patch_LocString.Format(item.Item2, otherName);
                }
            }
        }
        if (GetAtoms().Count == 4 && GetBonds().Count == 3 && IsStandardBond(GetBonds()[0]) && IsStandardBond(GetBonds()[1]) && IsStandardBond(GetBonds()[2])) {
            var atoms = GetAtoms();
            if (atoms.TryGetValue(new(0, 0), out var atom) && atom.atomType == AtomTypes.water &&
                atoms.TryGetValue(new(-1, 0), out atom) && atom.atomType == AtomTypes.salt &&
                atoms.TryGetValue(new(1, -1), out atom) && atom.atomType == AtomTypes.salt &&
                atoms.TryGetValue(new(0, 1), out atom) && atom.atomType == AtomTypes.fire
                ) return Translations.Translate("Distilled Alcohol");
        }
        return MaybeHelper.empty;
    }

    private bool IsStandardBond(Bond bond) {
        return bond.type == BondTypeEnum.Standard;
    }
}