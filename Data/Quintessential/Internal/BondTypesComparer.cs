using System.Collections.Generic;

namespace Quintessential.Internal;

internal class BondTypesComparer : IEqualityComparer<IReadOnlyList<BondType>> {
    public bool Equals(IReadOnlyList<BondType>? b1, IReadOnlyList<BondType>? b2) {
        if (ReferenceEquals(b1, b2)) return true;
        if (b2 is null || b1 is null) return false;
        if (b1.Count != b2.Count) return false;
        for (int i = 0; i < b1.Count; i++)
            if (b1[i].Id != b2[i].Id) return false;
        return true;
    }

    public int GetHashCode(IReadOnlyList<BondType> bond) {
        int bc = 0;
        foreach (var item in bond)
            bc ^= item.Id.GetHashCode();
        return bc;
    }
}