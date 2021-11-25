using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("class_93")]
class patch_TitleScreen {

	// renders main menu
	// adds notice mod count

	public static extern void orig_method_90(float param_3772, float param_3773, bool param_3774);
	public static void method_90(float param_3772, float time, bool renderText) {
		orig_method_90(param_3772, time, renderText);
		if(renderText) {
			class_135.method_290($"Quintessential v{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", new Vector2(49f, 100f), class_238.field_1990.field_2144, class_181.field_1718.WithAlpha(0.7f), 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			class_135.method_290($"{QuintessentialLoader.Mods.Count} mods loaded.", new Vector2(49f, 77f), class_238.field_1990.field_2144, class_181.field_1718.WithAlpha(0.7f), 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		}
	}
}