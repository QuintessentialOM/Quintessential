#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
using Quintessential;
using System;

class patch_class_103 {

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