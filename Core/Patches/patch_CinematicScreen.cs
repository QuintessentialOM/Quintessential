using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

internal class patch_CinematicScreen {
    [MonoModIgnore]
    private readonly LocalizedVignette vignette;

    [MonoModILInject("RenderFrame")]
    static void PatchBackground(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition getBackground = method.DeclaringType.Methods.First(m => m.Name.Equals("GetBackground"));
        MethodDefinition getTitle = method.DeclaringType.Methods.First(m => m.Name.Equals("GetCinematicTitle"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("TextureAssets", "transparent"));
        cursor.Index--;
        cursor.RemoveRange(2);
        cursor.EmitLdarg0();
        cursor.EmitCall(getBackground);

        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdsfld("System.String", "Empty"));
        cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitCall(getTitle);
    }

    Texture cachedBackground;
    Texture GetBackground() {
        if (cachedBackground != null) return cachedBackground;

        if (vignette == null) return Assets.textures.transparent;
        string path = ((patch_Vignette)(object)vignette.GetVignette()).backgroundPath;
        if (path == null) return Assets.textures.transparent;

        cachedBackground = AssetLoaderHelper.LoadTexture(path);
        return cachedBackground;
    }

    string GetCinematicTitle() {
        if (vignette == null) return string.Empty;
        return ((patch_Vignette)(object)vignette.GetVignette()).cinematicTitle ?? string.Empty;
    }

    [MonoModIgnore]
    public extern void orig_OnOpenOrClose(bool isOpening);
    public void OnOpenOrClose(bool isOpening) {
        orig_OnOpenOrClose(isOpening);
        if (cachedBackground != null) Renderer.DestroyTexture(cachedBackground);
        cachedBackground = null;
    }
}
