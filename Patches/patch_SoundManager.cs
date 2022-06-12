using System;
using System.Reflection;
using System.Collections.Generic;
using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_201")]

class patch_SoundManager
{
	public extern void orig_method_539(Action<int> param_3876);
	public void method_539(Action<int> param_3876)
	{
		orig_method_539(param_3876);

		//redo the volume dictionary
		var volumeDictField = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<string, float> oldVolumeDict = (Dictionary<string, float>)volumeDictField.GetValue(null);
		var newVolumeDict = new Dictionary<string, float>();
		foreach (var kvp in oldVolumeDict)
		{
			newVolumeDict.Add("sounds/" + kvp.Key, kvp.Value);
		}
		volumeDictField.SetValue(null, newVolumeDict);

		//prep sound dictionary
		QApi.initializeSoundDictionary();
	}

	public extern void orig_method_540();
	public void method_540()
	{
		orig_method_540();
		QApi.resetSounds();
	}
}