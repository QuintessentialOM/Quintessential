using System;
using System.Reflection;
using System.Collections.Generic;
using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_102")]

class patch_SongManager
{
	public extern void orig_method_129(Action<int> param_132);
	public void method_129(Action<int> param_132)
	{
		orig_method_129(param_132);

		//load mod songs
		QuintessentialLoader.LoadSongs();
	}
}