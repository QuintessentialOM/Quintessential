using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class patch_class_65 {

	[PatchSettingsStaticInit]
	public static extern void orig_cctor();

	[MonoModConstructor]
	public static void cctor() {
		orig_cctor();
	}
}