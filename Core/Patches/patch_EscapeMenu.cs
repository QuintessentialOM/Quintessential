using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("EscapeScreen")]
class patch_PauseScreen {
	public static readonly bool IsModsScreenEnabled = true;

	public extern void orig_RenderFrame(float deltaTime);

	public void RenderFrame(float deltaTime) {
		if(GameLogic.instance.GetCurrentScreen() is ModsScreen)
			return;
        orig_RenderFrame(deltaTime);
		if (IsModsScreenEnabled) { // Just here so that mods can overwrite this and disable the mods screen with a WrapOperation
            float num = 65f;
			Vector2 vector2_1 = new(570f, 440f);
			Vector2 vector2_2 = (InputManager.screenSize / 2 - vector2_1 / 2).Rounded();
			Vector2 vector2_3 = new(161f, 256f - num * 4);
			if(UIUtils.TextButton(Translations.Translate("Mods"), vector2_2 + vector2_3).RenderAndCheckIfPressed(true,true)) {
                // show mod options
                UI.OpenScreen(new ModsScreen());
			}
		}
	}
}

