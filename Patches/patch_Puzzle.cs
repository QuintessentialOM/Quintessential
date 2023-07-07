using System.Collections.Generic;
using MonoMod;

class patch_Puzzle{
	
	// Custom puzzle data
	public HashSet<string> CustomPermissions = new();
	
	// Is modded content allowed in this puzzle?
	// Controls whether this is saved to/from a vanilla `.puzzle` file, or a Quintessential `.puzzle.yaml` file
	public bool IsModdedPuzzle = false;
	
	// Set puzzle ID to 0
	[PatchPuzzleIdWrite]
	[MonoModIgnore]
	public static extern void method_1248(string param_4980);
}