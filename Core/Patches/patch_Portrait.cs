using MonoMod;

internal class patch_Portrait {
    [MonoModRemoveReadOnly]
    public Texture largeTexture;
    [MonoModRemoveReadOnly]
    public Texture smallTexture;
}
