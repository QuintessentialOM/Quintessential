using Quintessential.Internal;
using Quintessential.Serialization;
using System.IO;

namespace Quintessential;
public static class Dumping {

    internal static void DumpVanillaPuzzles() {
        string outDir = Path.Combine(QuintessentialLoader.PathModSaves, "Quintessential", "DumpedPuzzles");
        Directory.CreateDirectory(outDir);
        DataSerializer.SetMultilineFormat(true);
        foreach (var p in Puzzles.campaignPuzzles) {
            PuzzleModel m = PuzzleModel.FromPuzzle(p);
            m.Serialize(Path.Combine(outDir, m.ID + ".puzzle.jsonc"));
        }
        foreach (var volume in JournalVolumes.volumes) {
            foreach (var p in volume.puzzles) {
                PuzzleModel m = PuzzleModel.FromPuzzle(p);
                m.Serialize(Path.Combine(outDir, "X" + m.ID + ".puzzle.jsonc"));
            }
        }
        Logger.Log($"Dumped puzzles to {outDir}");
        UI.OpenScreen(new NoticeScreen(
            QuintessentialUI.Instance.Translate("dumping.puzzle"),
            QuintessentialUI.Instance.Translate("dumping.puzzle.tooltip") + " \"" + outDir.Replace('\\', '/') + "\""
        ));
    }

    internal static void DumpAtomSprites() {
        string outDir = Path.Combine(QuintessentialLoader.PathModSaves, "Quintessential", "DumpedAtomSprites");
        Directory.CreateDirectory(outDir);
        foreach (AtomType atomType in AtomTypes.atoms) {
            RenderTargetHandle v = RenderAtomToTarget(atomType);
            Renderer.PngFromTexture(v.GetTarget().renderedTexture).Save(Path.Combine(outDir, atomType.Id.ToString().Replace(":", "__") + ".png"));
        }
        Logger.Log($"Dumped atom sprites to {outDir}");
        UI.OpenScreen(new NoticeScreen(
            QuintessentialUI.Instance.Translate("dumping.atom_sprite"),
            QuintessentialUI.Instance.Translate("dumping.atom_sprite.tooltip") + " \"" + outDir.Replace('\\', '/') + "\""
        ));
    }
    internal static RenderTargetHandle RenderAtomToTarget(AtomType type) {
        RenderTargetHandle renderTargetHandle = new RenderTargetHandle();
        Bounds2 bounds = Bounds2.CenteredOn(HexGrid.standardGrid.ToPixelCoords(new HexIndex(0, 0), Vector2.Zero), HexGrid.standardGrid.hexSize.X, HexGrid.standardGrid.hexSize.Y * 1.3f);
        Index2 size = bounds.Size.CeilingToInt() + new Index2(20 * 2, 20 * 2);
        Vector2 pos = size.ToVector2() / 2 / 1f - bounds.Center;
        pos.Y = -pos.Y;
        renderTargetHandle.targetSize = size;
        RenderTarget class95 = renderTargetHandle.GetTarget(out var flag);
        if (flag) {
            using (class_226.method_597(class95, Matrix4.GetScale(new Vector3(1, -1, 1)))) {
                class_226.method_600(Color.Transparent);
                Editor.RenderAtom(type, pos, 1, 1, 1, 1, -21, 0, Assets.textures.white, null, false);
            }
        }

        return renderTargetHandle;
    }
}
