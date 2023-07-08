
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential;

class patch_MoleculeEditorScreen{

	internal patch_Puzzle editing;
	
	public extern void orig_method_50(float param);
	public void method_50(float param) {
		orig_method_50(param);
		if(editing is { IsModdedPuzzle: true }){ // find the correct position to put the atoms
			Vector2 uiSize = new(1516f, 922f);
			Vector2 corner = (Input.ScreenSize() / 2 - uiSize / 2 + new Vector2(-2f, -11f)).Rounded();
			Vector2 atomSize = new(95f, -90f);
			Vector2 atomPos = corner + new Vector2(169f, 754f); // vanilla atoms
			atomPos.X += atomSize.X * 3;
			foreach(var type in QApi.ModAtomTypes){
				method_1130(atomPos, type, true);
				atomPos.Y += atomSize.Y;
			}
		}
	}

	[MonoMod.MonoModIgnore]
	private extern void method_1130(Vector2 pos, AtomType type, bool b);
}