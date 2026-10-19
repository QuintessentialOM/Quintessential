using MonoMod;

internal class patch_Renderer {
    [MonoModIgnore] private static IPlatformRenderer platformRenderer;
    public static void DestroyRenderTargetRaw(RenderTarget renderTarget) {
        platformRenderer.DestroyRenderTarget(renderTarget.renderTarget);
    }
}
