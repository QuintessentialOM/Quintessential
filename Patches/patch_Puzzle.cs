using System.Collections.Generic;
using System.IO;
using MonoMod;
using Quintessential;
using Quintessential.Serialization;

class patch_Puzzle{
	
	// Custom puzzle data
	public HashSet<string> CustomPermissions = new();
	
	// Is modded content allowed in this puzzle?
	// Controls whether this is saved to/from a vanilla `.puzzle` file, or a Quintessential `.puzzle.yaml` file
	public bool IsModdedPuzzle = false;
	
	// Save using the right format, and set Steam user ID to 0
	[PatchPuzzleIdWrite]
	public extern void orig_method_1248(string path);
	public void method_1248(string path){
		if(IsModdedPuzzle)
			File.WriteAllText(path, YamlHelper.Serializer.Serialize(PuzzleModel.FromPuzzle((Puzzle)(object)this)));
		else
			orig_method_1248(path);
	}
}