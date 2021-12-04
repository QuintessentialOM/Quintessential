using SDL2;

namespace Quintessential.Settings {

	class ChangeKeybindScreen : IScreen {

		Keybinding Key;
		string Label;
		QuintessentialMod ToSave;

		// SDL doesn't make an event when Control or Alt are pressed unless it makes a character (or maybe OM doesn't pick it up)
		// So we just use this
		public char[] BindableKeys = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-+_=/*!\"£$%^&()<>,.?{}[]:;@'~#|\\`¬¦".ToCharArray();

		public ChangeKeybindScreen(Keybinding key, string label, QuintessentialMod save) {
			Key = key;
			Label = label;
			ToSave = save;
		}

		public bool method_1037() {
			return false;
		}
		public void method_47(bool param_4687) {
			// Add gray BG
			GameLogic.field_2434.field_2464 = true;
		}

		public void method_48() {}

		public void method_50(float param_4686) {
			// "Please enter a new key:"
			UI.DrawText("Please enter a new key for: " + Label, (Input.ScreenSize() / 2) + new Vector2(0, 170), UI.Title, Color.White, TextAlignment.Centred);
			// display ctrl/shift
			string preview = "";
			bool shift = Input.IsShiftHeld();
			bool ctrl = Input.IsControlHeld();
			bool alt = Input.IsAltHeld();
			if(shift)
				preview = "Shift + " + preview;
			if(alt)
				preview = "Alt + " + preview;
			if(ctrl)
				preview = "Control + " + preview;
			if(!string.IsNullOrWhiteSpace(preview))
				UI.DrawText(preview, Input.ScreenSize() / 2, UI.Title, class_181.field_1718, TextAlignment.Centred);
			// "press esc to CANCEL"
			Bounds2 labelBounds = UI.DrawText("Press ESC to ", (Input.ScreenSize() / 2) + new Vector2(0, -170), UI.SubTitle, class_181.field_1718, TextAlignment.Centred);
			if(Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE) || UI.DrawAndCheckSimpleButton("CANCEL", labelBounds.BottomRight + new Vector2(10, 0), new Vector2(70, (int)labelBounds.Height + 10)))
				UI.HandleCloseButton();
			// handle keypresses
			char key = char.MinValue;
			foreach(var bindable in BindableKeys)
				if(Input.IsKeyPressed(bindable.ToString()))
					key = bindable;
			char upper = char.ToUpper(key);
			if(upper != char.MinValue) {
				Keybinding old = Key.Copy();
				Key.Key = upper.ToString();
				Key.Shift = shift;
				Key.Control = ctrl;
				Key.Alt = alt;
				Logger.Log($"Changed keybind for \"{Label}\": from \"{old}\" to \"{Key}\".");
				ModsScreen.SaveSettings(ToSave);
				UI.CloseScreen();
			}
		}
	}
}