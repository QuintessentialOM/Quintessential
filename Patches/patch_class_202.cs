using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
class patch_class_202 {

	// Render main menu
	public static extern void orig_method_516(float param_3772, float param_3773, bool param_3774);
	public static void method_516(float param_3772, float time, bool renderText) {
		orig_method_516(param_3772, time, renderText);
		if(renderText)
			class_290.method_800($"Quintessential v{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", new Vector2(49f, 70f), class_227.field_2042.field_149, class_36.field_284.WithAlpha(0.7f), 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
	}
}