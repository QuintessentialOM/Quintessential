#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
using Quintessential;
using System;

class patch_class_283 {

	public static extern void orig_method_753();
	public static void method_753() {
		AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
			Logger.Log("Encountered an error!");
			Exception e = args.ExceptionObject as Exception;
			Logger.Log(e.Message);
			Logger.Log(e.StackTrace);
		};
	}
}