using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_201")]

class patch_SoundReset
{
	public extern void orig_method_540();
	public void method_540()
	{
		orig_method_540();
		QApi.resetSounds();
	}
}