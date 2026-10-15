using MonoMod;
using Quintessential;
using Quintessential.Internal;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("TitleScreen")]
class patch_TitleScreen {

	// renders main menu
	// adds notice mod count

	public static extern void orig_Render(float loadCompletion, float animationTime, bool displayText);
	public static void Render(float loadCompletion, float animationTime, bool displayText) {
        orig_Render(loadCompletion, animationTime, displayText);
		if(displayText) {
			TextureRenderer.RenderText(QuintessentialCore.Instance.Translate("simple") + " v" + QuintessentialCore.Instance.Meta.Version, new Vector2(49f, 100f), Assets.fonts.crimson_15, UIConsts.lightTextColor.WithAlpha(0.7f), 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
            TextureRenderer.RenderText(QuintessentialLoader.Mods.Count + " " + QuintessentialCore.Instance.Translate("display_text.mods_loaded"), new Vector2(49f, 77f), Assets.fonts.crimson_15, UIConsts.lightTextColor.WithAlpha(0.7f), 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
        }
	}
}