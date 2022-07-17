using System;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
class patch_GameLogic {

	// calls mod loading
	public extern void orig_method_942();

	public void method_942() {
		QuintessentialLoader.PreInit();
		orig_method_942();
		QuintessentialLoader.PostLoad();
	}

	public extern void orig_method_963(int exitCode);

	public void method_963(int exitCode) {
		QuintessentialLoader.Unload();
		orig_method_963(exitCode);
	}

	public extern void orig_method_956();

	public void method_956() {
		orig_method_956();
		QuintessentialLoader.LoadPuzzleContent();
	}

	private extern void orig_method_955(Action<int> param_4624);

	private void method_955(Action<int> param_4624)
	{
		orig_method_955(param_4624);
		QApi.initializeFontDictionary();
		patch_DocumentScreen.initializeDocumentDictionary();
	}

}