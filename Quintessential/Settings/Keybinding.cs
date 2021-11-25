using SDL2;

namespace Quintessential.Settings {
	
	public class Keybinding {

		// only one character
		public string Key = "";

		public bool Shift = false, Control = false, Alt = false;

		public bool IsControlKeysPressed() {
			return (!Shift || Input.IsShiftHeld()) && (!Control || Input.IsControlHeld()) && (!Alt || Input.IsAltHeld());
		}

		public bool Pressed() {
			return IsControlKeysPressed() && Input.IsKeyPressed(Key);
		}

		public bool Held() {
			return IsControlKeysPressed() && Input.IsKeyHeld(Key);
		}

		public bool Released() {
			return IsControlKeysPressed() && Input.IsKeyReleased(Key);
		}
	}
}
