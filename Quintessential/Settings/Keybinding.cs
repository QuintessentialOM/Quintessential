using SDL2;
using YamlDotNet.Serialization;

namespace Quintessential.Settings {
	
	public class Keybinding {

		// only one character
		public string Key = "";

		public bool Shift = false, Control = false, Alt = false;

		public bool Pressed() {
			return (!Shift || class_115.method_193(0)) && (!Control || class_115.method_193((enum_143)1)) && (!Alt || class_115.method_193((enum_143)2))
				&& class_115.method_198(SDL.SDL_GetKeyFromName(Key));
		}
	}
}
