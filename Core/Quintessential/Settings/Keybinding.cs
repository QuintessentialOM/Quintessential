using SDL2;

namespace Quintessential.Settings;

public class Keybinding {

	// only one character
	public string Key = "";

	public bool Shift = false, Control = false, Alt = false;

	public Keybinding(){}

	public Keybinding(string key, bool shift = false, bool control = false, bool alt = false){
		Key = key;
		Shift = shift;
		Control = control;
		Alt = alt;
	}

	public bool IsControlKeysPressed(){
		return (!Shift || InputManager.IsModifierKeyHeld(ModifierKeyType.Shift)) &&
			(!Control || InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) &&
			(!Alt || InputManager.IsModifierKeyHeld(ModifierKeyType.Alt));
	}

	public bool Pressed(){
		return IsControlKeysPressed() && InputManager.IsKeyPressed(SDL.SDL_GetKeyFromName(Key));
	}

	public bool Held(){
		return IsControlKeysPressed() && InputManager.IsKeyHeld(SDL.SDL_GetKeyFromName(Key));
	}

	public bool Released(){
		return IsControlKeysPressed() && InputManager.IsKeyReleased(SDL.SDL_GetKeyFromName(Key));
	}

	public Keybinding Copy() {
        Keybinding copy = new() {
            Key = Key,
            Shift = Shift,
            Control = Control,
            Alt = Alt
        };
        return copy;
	}

	public string ControlKeysText() {
		return (Control ? "Control + " : "") + (Alt ? "Alt + " : "") + (Shift ? "Shift + " : "");
	}

	public override string ToString() {
		return ControlKeysText() + Key;
	}
}
