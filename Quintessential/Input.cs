using SDL2;

namespace Quintessential;

using OMInput = class_115;
using SdlKey = SDL.enum_160;

/// <summary>
/// Helper class containing functions for querying keyboard or mouse input.
/// </summary>
public static class Input {

	#region Keyboard input

	public static bool IsShiftHeld() {
		return OMInput.method_193(0);
	}

	public static bool IsControlHeld() {
		return OMInput.method_193((enum_143)1);
	}

	public static bool IsAltHeld() {
		return OMInput.method_193((enum_143)2);
	}

	public static bool IsSdlKeyPressed(SdlKey key) {
		return OMInput.method_198(key);
	}

	public static bool IsSdlKeyReleased(SdlKey key) {
		return OMInput.method_199(key);
	}

	public static bool IsSdlKeyHeld(SdlKey key) {
		return OMInput.method_200(key);
	}

	public static SdlKey GetSdlKeyForCharacter(string character) {
		return SDL.SDL_GetKeyFromName(character);
	}

	public static bool IsKeyPressed(string key) {
		return OMInput.method_198(GetSdlKeyForCharacter(key));
	}

	public static bool IsKeyReleased(string key) {
		return OMInput.method_199(GetSdlKeyForCharacter(key));
	}

	public static bool IsKeyHeld(string key) {
		return OMInput.method_200(GetSdlKeyForCharacter(key));
	}

	#endregion

	#region Mouse input

	public static Vector2 MousePos() {
		return OMInput.method_202();
	}

	// Not 100% sure on other clicks so I won't include them yet

	public static bool IsLeftClickPressed() {
		return OMInput.method_206((enum_142)1);
	}

	public static bool IsLeftClickReleased() {
		return OMInput.method_207((enum_142)1);
	}

	public static bool IsLeftClickHeld() {
		return OMInput.method_205((enum_142)1);
	}

	#endregion

	#region Other

	public static Vector2 ScreenSize() {
		return OMInput.field_1433;
	}

	#endregion
}
