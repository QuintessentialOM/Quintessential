using SDL2;

namespace Quintessential.Settings {
	
	public class Keybinding {

		// only one character
		public string Key = "";

		public bool Shift = false, Control = false, Alt = false;

		public Keybinding(){}

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

		public Keybinding Copy() {
			Keybinding copy = new Keybinding();
			copy.Key = Key;
			copy.Shift = Shift;
			copy.Control = Control;
			copy.Alt = Alt;
			return copy;
		}

		public string ControlKeysText() {
			return (Control ? "Control + " : "") + (Alt ? "Alt + " : "") + (Shift ? "Shift + " : "");
		}

		public override string ToString() {
			return ControlKeysText() + Key;
		}
	}
}
