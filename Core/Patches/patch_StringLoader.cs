using MonoMod;
using Quintessential;
using Quintessential.Internal;
using System;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("StringLoader")]
class patch_StringLoader {

	// string loader patches
	// should never be called

	public static extern string orig_GetObfuscatedStringById(int idc);

	public static string GetObfuscatedStringById(int n) {
		try {
			return orig_GetObfuscatedStringById(n);
		} catch(Exception e) {
			QuintessentialCore.Instance?.Log?.Error("Missing text key: " + n + "!");
			QuintessentialCore.Instance?.Log?.Error(e);

			return "!!! quintessential: missing string !!!";
		}
	}
}