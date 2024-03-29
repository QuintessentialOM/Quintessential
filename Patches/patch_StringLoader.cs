﻿using MonoMod;
using Quintessential;
using System;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("class_103")]
class patch_StringLoader {

	// string loader patches
	// should never be called

	public static extern string orig_method_131(int idc);

	public static string method_131(int n) {
		try {
			return orig_method_131(n);
		} catch(Exception e) {
			if(Logger.Setup) {
				Logger.Log("Missing text key: " + n + "!");
				Logger.Log(e);
			}

			return "!!! quintessential: missing string !!!";
		}
	}
}