using System.Collections.Generic;
using MonoMod;

class patch_Puzzle{
	
	// Custom puzzle data
	public HashSet<string> CustomPermissions = new();

	// Set puzzle ID to 0
	[PatchPuzzleIdWrite]
	[MonoModIgnore]
	public static extern void method_1248(string param_4980);
}