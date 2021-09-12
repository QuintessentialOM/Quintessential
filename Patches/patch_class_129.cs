#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
using Quintessential;
using System;

class patch_class_129 {

	public static extern void orig_method_238();
	public static void method_238() {
		AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
			Logger.Log("Encountered an error!");
			Exception e = args.ExceptionObject as Exception;
			Logger.Log(e.ToString());
		};
	}
}