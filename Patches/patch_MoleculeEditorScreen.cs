
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_MoleculeEditorScreen {

	public extern void orig_method_50(float param);

	public void method_50(float param) {
		orig_method_50(param);
		// TODO: actually put them in the right place, and only appear on modded puzzles
		Vector2 pos = new Vector2(410, 640);
		foreach(var type in Quintessential.QApi.ModAtomTypes) {
			method_1130(pos, type, true);
			pos.Y += 100;
		}
	}

	[MonoMod.MonoModIgnore]
	private extern void method_1130(Vector2 pos, AtomType type, bool b);
}