using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("class_172")]

class patch_VignetteCast
{
	public static extern void orig_method_480();

	public static void method_480()
	{
		orig_method_480();
		QuintessentialLoader.LoadVignetteActors();
	}
}