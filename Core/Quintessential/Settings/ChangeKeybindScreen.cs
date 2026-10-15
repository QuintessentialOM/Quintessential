using System.Collections.Generic;
using Quintessential.Internal;
using SDL2;

namespace Quintessential.Settings;

class ChangeKeybindScreen : IScreen {

	Keybinding Key;
    LocString Label;
	QuintessentialMod ToSave;

	// SDL doesn't make an event when Control or Alt are pressed unless it makes a character (or maybe OM doesn't pick it up)
	// So we just use this
	public static List<string> BindableKeys = [];

	static ChangeKeybindScreen(){
		foreach(var letter in "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-+_=/*!\"£$%^&()<>,.?{}[]:;@'~#|\\`¬¦".ToCharArray())
			BindableKeys.Add(letter.ToString());
		for(int i = 1; i < 25; i++) // F1 -> F24
			BindableKeys.Add("F" + i);
		for(int i = 0; i < 10; i++) // Keypad numbers
			BindableKeys.Add("Keypad " + i);
		BindableKeys.AddRange(["Insert", "PageUp", "PageDown", "Home", "End"]);
	}

	public ChangeKeybindScreen(Keybinding key, LocString label, QuintessentialMod save){
		Key = key;
		Label = label;
		ToSave = save;
	}

	public bool PreventLowerScreenUpdates(){
		return false;
	}
	public void OnOpenOrClose(bool isOpening) {
		// Add gray BG
		GameLogic.instance.fadeBackground = true;
	}

	public void Reset(){}

	public void RenderFrame(float deltaTime) {
		// "Please enter a new key:"
		UI.DrawText(QuintessentialCore.Instance.Translate("display_text.key_prompt") + " " + Label, (InputManager.screenSize / 2) + new Vector2(0, 170), UI.Title, Color.White, TextAlignment.Center);
        // display ctrl/shift
        string preview = "";
		bool shift = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
        bool ctrl = InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl);
        bool alt = InputManager.IsModifierKeyHeld(ModifierKeyType.Alt);
        if (shift)
			preview = "Shift + " + preview;
		if(alt)
			preview = "Alt + " + preview;
		if(ctrl)
			preview = "Control + " + preview;
		if(!string.IsNullOrWhiteSpace(preview))
			UI.DrawText(preview, InputManager.screenSize / 2, UI.Title, UIConsts.lightTextColor, TextAlignment.Center);
		// "press esc to CANCEL"
		Bounds2 labelBounds = UI.DrawText(QuintessentialCore.Instance.Translate("display_text.escape_prompt") + " ", (InputManager.screenSize / 2) + new Vector2(-40, -170), UI.SubTitle, UIConsts.lightTextColor, TextAlignment.Center);
        if (InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE) || UI.DrawAndCheckSimpleButton(QuintessentialCore.Instance.Translate("display_text.cancel_keybind"), labelBounds.BottomRight + new Vector2(10, -7), new Vector2(70, (int)labelBounds.Height + 10)))
			UI.HandleCloseButton();
		// handle keypresses
		string key = "";
		foreach(var bindable in BindableKeys)
			if(InputManager.IsKeyPressed(SDL.SDL_GetKeyFromName(bindable)))
				key = bindable;
		if(key != ""){
			Keybinding old = Key.Copy();
			Key.Key = key.Length == 1 ? key.ToUpper() : key; // make all letters uppercase, but keep e.g. PageUp
			Key.Shift = shift;
			Key.Control = ctrl;
			Key.Alt = alt;
			Logger.Log($"Changed keybind for \"{Label}\": from \"{old}\" to \"{Key}\".");
			ModsScreen.SaveSettings(ToSave);
			UI.CloseScreen();
		}
	}
}
