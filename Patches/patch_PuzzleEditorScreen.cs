#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

using MonoMod;

class patch_PuzzleEditorScreen{

	[MonoModIgnore]
	[PatchPuzzleEditorScreen]
	public extern void method_50(float param);

	// name is used in MonoModRules
	private void DisplayEditorPanelScreen(){
		// reimplement this section

	}
}