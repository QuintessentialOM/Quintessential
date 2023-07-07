// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quintessential;
using Quintessential.Serialization;

internal class patch_WorkshopManager{
	
	public void method_2230(){
		// without the object cast, it's illegal
		((WorkshopManager)(object)this).method_2234();
		((WorkshopManager)(object)this).method_2235();
	}

	// make the Browse button a no-op rather than crashing
	public void method_2233(){}

	// load YAML-based puzzles alongside binary ones
	private extern IEnumerable<Puzzle> orig_method_2236(string folder);
	private IEnumerable<Puzzle> method_2236(string folder){
		return orig_method_2236(folder).Concat(YamlPuzzles(folder));
	}

	private static IEnumerable<Puzzle> YamlPuzzles(string folder){
		string path = Path.Combine(class_269.field_2102, folder);
		foreach(var puzzleFilePath in Directory.EnumerateFiles(path, "*.puzzle.yaml")){
			PuzzleModel model = YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(puzzleFilePath));
			Puzzle fromModel = PuzzleModel.FromModel(model);
			// ReSharper disable once PossibleInvalidCastException
			((patch_Puzzle)(object)fromModel).IsModdedPuzzle = true;
			yield return fromModel;
		}
	}
}