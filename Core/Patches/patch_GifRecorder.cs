#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

using MonoMod;
using Quintessential.Internal;
using System.IO;

[MonoModPatch("GifRecorder")]
class patch_GifRecorder {

    [MonoModWrapOperation("LoadPaletteData", "Literal-String", "Content")]
    private static string GetPaletteDataPath(string orig) {
        return Path.Combine(QuintessentialCore.Instance.Meta.PathToDirectory, orig);
    }
}