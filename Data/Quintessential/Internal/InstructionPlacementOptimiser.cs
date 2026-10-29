using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintessential.Internal;

internal class InstructionPlacementOptimiser {

    public record InstructionTray {
        public Dictionary<InstructionPosition, InstructionType> Tray;
        internal bool CanRelocate;
        internal (int, int) Size;
        internal (int, int) Position;
    }

    internal static void CompactInstructionTrays() {
        List<InstructionTray> trays = QApi.InstructionTrays;
        QApi.InstructionTrays = [];
        foreach (var tray in trays)
            if (tray.Size == default) ComputeSize(tray);

        trays = [.. trays.OrderByDescending(tray => tray.Size).Where(tray => tray.Size.Item1 != 0 && tray.Size.Item2 != 0)];

        while (trays.Count > 0 ) {
            var tray = trays[0];
            trays.RemoveAt(0);

            bool found;
            do {
                found = false;
                int i = 0;
                (int, int) offset = (0, 0);
                for (; i < trays.Count; i++) {
                    if (CanTrayFit(tray, trays[i], ref offset)) {
                        found = true;
                        break;
                    }
                }
                if (found) {
                    tray.Tray.AddRange( trays[i].Tray.Select(pair => {
                        return KeyValuePair.Create((InstructionPosition)(((int)pair.Key) + offset.Item1 * 5 + offset.Item2), pair.Value);
                    }).ToDictionary());
                    trays.RemoveAt(i);
                }
            } while (found);

            for (int i = 0; i < 15; i++) {
                if (i == 10) continue;
                if (!tray.Tray.ContainsKey((InstructionPosition)i))
                    tray.Tray.Add((InstructionPosition)i, null);
            }
            QApi.InstructionTrays.Add(tray);
        }
    }

    private static bool CanTrayFit(InstructionTray destination, InstructionTray toFit, ref (int, int) offset) {
        if (!toFit.CanRelocate) {
            offset = (0, 0);
            return !destination.Tray.Keys.Intersect(toFit.Tray.Keys).Any();
        }
        IEnumerable<int> destKeys = destination.Tray.Keys.Select(pos => (int)pos).Append(10);
        IEnumerable<int> fitKeys = toFit.Tray.Keys.Select(pos => (int)pos);

        for (int i = (5 - toFit.Size.Item2); i >= 0; i--) {
            for (int j = 0; j <= (3 - toFit.Size.Item1); j++) {
                var posOffset = (j - toFit.Position.Item1, i - toFit.Position.Item2);

                if (!fitKeys.Any(pos => destKeys.Contains( pos + (posOffset.Item1 * 5 + posOffset.Item2)))) {
                    offset = posOffset;
                    return true;
                }
            }
        }
        return false;
    }

    internal static void ComputeSize(InstructionTray tray) {
        if (tray.Tray.Count == 0) {
            tray.Position = (0, 0);
            tray.Size = (0, 0);
            return;
        }
        int rowMax = 0;
        int rowMin = 2;
        int colMax = 0;
        int colMin = 4;
        foreach (var item in tray.Tray) {
            int i = (int)item.Key;
            int col = i % 5;
            int row = i / 5;
            rowMax = Math.Max(row, rowMax);
            rowMin = Math.Min(row, rowMin);
            colMax = Math.Max(col, colMax);
            colMin = Math.Min(col, colMin);
        }
        tray.Position = (rowMin, colMin);
        tray.Size = (rowMax - rowMin + 1, colMax - colMin + 1);
    }
}
