using MonoMod;

class patch_Puzzle {

	// sets your puzzle ID to 0
	// will be improved in the future

	[PatchPuzzleIdWrite]
	[MonoModIgnore]
	public static extern void method_1248(string param_4980);
}