using SDL2;

namespace Quintessential.Settings {

	class ChangeKeybindScreen : IScreen {

		Keybinding Key;

		// SDL doesn't make an event when Control or Alt are pressed unless it makes a character (or maybe OM doesn't pick it up)
		// So we just use this
		public char[] BindableKeys = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-+_=/*!\"£$%^&()<>,.?{}[]:;@'~#|\\`¬¦".ToCharArray();

		public ChangeKeybindScreen(Keybinding key) {
			Key = key;
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
			class_135.method_290("Please enter a new key:", (Input.ScreenSize() / 2) + new Vector2(0, 170), class_238.field_1990.field_2146, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
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
				class_135.method_290(preview, Input.ScreenSize() / 2, class_238.field_1990.field_2146, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
			// "press esc to CANCEL"
			Bounds2 labelBounds = class_135.method_290("Press ESC to ", (Input.ScreenSize() / 2) + new Vector2(0, -170), class_238.field_1990.field_2143, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
			if(Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE) || class_140.class_149.method_348("CANCEL", labelBounds.BottomRight + new Vector2(10, 0), new Vector2(70, (int)labelBounds.Height + 10)).method_824(true, true)) {
				GameLogic.field_2434.field_2464 = false;
				GameLogic.field_2434.method_949();
				class_238.field_1991.field_1873.method_28(1f);
			}
			// handle keypresses
			char key = char.MinValue;
			foreach(var bindable in BindableKeys)
				if(Input.IsKeyPressed(bindable.ToString()))
					key = bindable;
			char upper = char.ToUpper(key);
			if(upper != char.MinValue) {
				Key.Key = upper.ToString();
				Key.Shift = shift;
				Key.Control = ctrl;
				Key.Alt = alt;
				GameLogic.field_2434.field_2464 = false;
				GameLogic.field_2434.method_949();
			}
		}
	}
}