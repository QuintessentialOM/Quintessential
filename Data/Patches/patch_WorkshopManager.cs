using MonoMod;
using Quintessential;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
internal class patch_WorkshopManager{
	//public void method_2230(){
	//	((WorkshopManager)(object)this).ReloadWorkshopPuzzles();
	//	((WorkshopManager)(object)this).ReloadCustomPuzzles();
	//}

    // load YAML-based puzzles alongside binary ones
    [MonoModIgnore] private extern IEnumerable<Puzzle> orig_LoadPuzzlesOfFolder(string folder);
	private IEnumerable<Puzzle> LoadPuzzlesOfFolder(string folder){
        var orig = orig_LoadPuzzlesOfFolder(folder).ToList();
        int originalCount = orig.Count;

        string path = Path.Combine(class_269.field_2102, folder);
        var computeFilehash = typeof(WorkshopManager).GetMethod("ComputeFileHash", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var puzzleFilePath in Directory.EnumerateFiles(path, "*.puzzle.json")) {
            try {
                Puzzle puzzle = Codecs.PUZZLE.Decode(JsonCodecMap.Instance, JsonNode.Parse(File.ReadAllText(puzzleFilePath)));
                puzzle.fileHash = (uint)computeFilehash.Invoke(GameLogic.instance.workshopManager, [puzzleFilePath]);

                orig.Add(puzzle);
            } catch (Exception e) {
                Logger.Log($"Exception loading custom puzzle from path \"{ Path.GetRelativePath(class_269.field_2102, puzzleFilePath) }\":");
                Logger.Log(e.Message);
                continue;
            }
        }

        for (int i = 0; i < originalCount; i++) {
            // delete, update, save
            File.Delete(Path.Combine(class_269.field_2102, folder, orig[i].puzzleId + ".puzzle"));
            orig[i].SaveToFile(((patch_WorkshopManager)(object)GameLogic.instance.workshopManager).CustomPuzzlePath(orig[i], folder));
        }
        return orig;
	}

    // give JSON-based puzzles the right file location
    [MonoModPublic] [MonoModReplace]
    public string CustomPuzzlePath(Puzzle puzzle){
        return Path.Combine(class_269.field_2102, "custom", puzzle.puzzleId + ".puzzle.json");
        //return puzzle.IsModdedPuzzle
        //    ? Path.Combine(class_269.field_2102, "custom", puzzle.puzzleId + ".puzzle.json")
        //    : orig_CustomPuzzlePath(puzzle);
    }
    public string CustomPuzzlePath(Puzzle puzzle, string folder) {
        return Path.Combine(class_269.field_2102, folder, puzzle.puzzleId + ".puzzle.json");
    }
}