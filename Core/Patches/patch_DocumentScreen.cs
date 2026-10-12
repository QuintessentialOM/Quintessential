using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

internal class patch_DocumentScreen {
    // TODO
    //[MonoModIgnore]
    //private readonly LocalizedVignette vignette;

    //[MonoModILInject("RenderFrame")]
    //static void PatchBackground(MethodDefinition method, CustomAttribute attribute) {
    //    ILCursor cursor = new(new ILContext(method));
    //    MethodDefinition getDocument = method.DeclaringType.Methods.First(m => m.Name.Equals("GetDocument"));
    //}

    //Texture cachedTexture;
    //Texture GetDocument() {
    //    if (cachedTexture != null) return cachedTexture;

    //    cachedTexture = AssetLoaderHelper.LoadTexture(path);
    //    return cachedTexture;
    //}

    //[MonoModIgnore]
    //public extern void orig_OnOpenOrClose(bool isOpening);
    //public void OnOpenOrClose(bool isOpening) {
    //    orig_OnOpenOrClose(isOpening);
    //    if (cachedTexture != null) Renderer.DestroyTexture(cachedTexture);
    //    cachedTexture = null;
    //}
}
