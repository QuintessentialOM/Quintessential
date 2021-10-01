#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
using MonoMod;
using Quintessential;
using System;

[MonoModPatch("class_129")]
class patch_ErrorHandler {

	// error logging
	// replaces the regular method (opening a (broken by string parsing?) website) with logging

	public static extern void orig_method_238();
	public static void method_238() {
		AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
			Logger.Log("Encountered an error!");
			Exception e = args.ExceptionObject as Exception;
			Logger.Log(e.ToString());
		};
	}
}