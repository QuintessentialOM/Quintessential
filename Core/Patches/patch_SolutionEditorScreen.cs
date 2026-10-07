using MonoMod;
using System.Collections.Generic;

class patch_SolutionEditorScreen
{
    /*[MonoModReplace]
    public static HexIndex method_2130(Solution solution, Part part)
    {
        // not the prettiest, but now it won't crash!
        HexIndex primaryResut = new(0, 0);
        HexIndex secondaryResult = new(0, 0);
        static int dot(HexIndex h1, HexIndex h2) => (2 * h1.Q + h1.R) * (2 * h2.Q + h2.R) + 3 * h1.R * h2.R;
        int maxDistance = 0;
        int bestX = int.MinValue;
        int bestY = int.MinValue;
        foreach (HexIndex offset in GetInputOutputConduitHexes(solution, part))
        {
            bool alignedFlag = offset.Q == 0 || offset.R == 0 || offset.ImpliedS == 0;
            bool fartherFlag = offset.Length() > primaryResut.Length();
            bool rightmost = offset.Q > primaryResut.Q;
            if (alignedFlag && (fartherFlag || rightmost))
            {
                primaryResut = offset;
            }
            for (int i = 0; i < 3; i++)
            {
                HexIndex dir = HexIndex.AdjacentOffsets[i];
                int dist = dot(dir, offset);
                int actDist = dist >> 2;
                int absDist = dist > 0 ? dist : -dist;
                HexIndex potentialHandle = new(actDist * dir.Q, actDist * dir.R);
                int x = 2 * potentialHandle.Q + potentialHandle.R;
                if (absDist > maxDistance
                    || (absDist == maxDistance && (x > bestX
                    || (x == bestX && potentialHandle.R > bestY)
                )))
                {
                    secondaryResult = potentialHandle;
                    maxDistance = absDist;
                    bestX = x;
                    bestY = potentialHandle.R;
                }
            }
        }
        if (primaryResut.Q == 0 && primaryResut.R == 0)
        {
            if (maxDistance == 0)
            {
                return new HexIndex(1, 0);
            }
            return secondaryResult;
        }
        return primaryResut;
    }*/

    [MonoModIgnore]
    public static extern HashSet<HexIndex> GetInputOutputConduitHexes(Solution solution, Part part);

}