internal class patch_TextureRenderer {
    public static void RenderScaled(Texture texture, Color color, Vector2 pos, OrientationMode orientation, FlippingMode flipping) {
        Vector2 scaledSize = texture.GetScale() * 2;
        Vector2 size = ((orientation == OrientationMode.Rotate90 || orientation == OrientationMode.Rotate270) ? new Vector2(scaledSize.Y, scaledSize.X) : new Vector2(scaledSize.X, scaledSize.Y));
        TextureRenderer.Render(texture, color, pos, size, Bounds2.WithSize(0f, 0f, 1f, 1f), orientation, flipping);
    }
}
