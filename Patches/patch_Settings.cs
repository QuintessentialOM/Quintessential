using MonoMod;

[MonoModPatch("class_110")]
class patch_Settings {

	// settings init
	// disabling steam

	[PatchSettingsStaticInit]
	public static extern void orig_cctor();

	[MonoModConstructor]
	public static void cctor() {
		orig_cctor();
	}
}