using MonoMod;
using Quintessential;
using System;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_17")]

class patch_TextureLoader
{
	public extern void orig_method_52(Action<int, int> param_65);
	public void method_52(Action<int, int> param_65)
	{
		orig_method_52(param_65);
		QApi.initializeTextureDictionary();
		QuintessentialLoader.LoadLocations();
	}
}