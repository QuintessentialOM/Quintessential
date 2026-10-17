using MonoMod;
using Quintessential;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
internal class patch_WorkshopManager {
    [MonoModIgnore] private extern IEnumerable<Puzzle> orig_LoadPuzzlesOfFolder(string folder);
    private IEnumerable<Puzzle> LoadPuzzlesOfFolder(string folder) {
        var orig = orig_LoadPuzzlesOfFolder(folder).ToList();
        int originalCount = orig.Count;

        string path = Path.Combine(PlatformBridge.savePath, folder);
        var computeFilehash = typeof(WorkshopManager).GetMethod("ComputeFileHash", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        string[] ExtraFileExtensions = {
            ".json",
            ".jsonc"
        };
        foreach (var extension in ExtraFileExtensions) {
            foreach (var puzzleFilePath in Directory.EnumerateFiles(path, "*.puzzle" + extension)) {
                try {
                    string text = extension == ".jsonc" ? DataSerializer.JsoncToJson(File.ReadAllText(puzzleFilePath)) : File.ReadAllText(puzzleFilePath);
                    Puzzle puzzle = Codecs.PUZZLE.Decode(JsonCodecMap.Instance, JsonNode.Parse(text));
                    ((patch_Puzzle)(object)puzzle).ExtraFileExtension = extension;
                    puzzle.fileHash = (uint)computeFilehash.Invoke(GameLogic.instance.workshopManager, [puzzleFilePath]);

                    orig.Add(puzzle);
                } catch (Exception e) {
                    Logger.Log($"Exception loading custom puzzle from path \"{Path.GetRelativePath(PlatformBridge.savePath, puzzleFilePath)}\":");
                    Logger.Log(e.Message);
                    continue;
                }
            }
        }

        for (int i = 0; i < originalCount; i++) {
            ((patch_Puzzle)(object)orig[i]).ExtraFileExtension = "";
            // delete, update, save
            //File.Delete(Path.Combine(PlatformBridge.savePath, folder, orig[i].puzzleId + ".puzzle"));
            //orig[i].SaveToFile(((patch_WorkshopManager)(object)GameLogic.instance.workshopManager).CustomPuzzlePath(orig[i], folder));
        }

        // PortLegacy
        foreach (var puzzleFilePath in Directory.EnumerateFiles(path, "*.puzzle.yaml")) {
            try {
                if (!QuintessentialLoader.TryLoadPuzzle(Path.GetDirectoryName(puzzleFilePath), Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(puzzleFilePath)), "", out Puzzle puzzle)) continue;
                patch_LegacyPorting.ExportPuzzleNewFormat(puzzleFilePath, patch_LegacyPorting.ExportFolder, puzzle);
            } catch (Exception e) {
                Logger.Log($"Exception loading legacy puzzle from path \"{Path.GetRelativePath(PlatformBridge.savePath, puzzleFilePath)}\":");
                Logger.Log(e);
                continue;
            }
        }

        return orig;
    }

    [MonoModPublic]
    [MonoModReplace]
    public string CustomPuzzlePath(Puzzle puzzle) {
        return Path.Combine(PlatformBridge.savePath, "custom", puzzle.puzzleId + ".puzzle" + ((patch_Puzzle)(object)puzzle).ExtraFileExtension);
    }
    public string CustomPuzzlePath(Puzzle puzzle, string folder) {
        return Path.Combine(PlatformBridge.savePath, folder, puzzle.puzzleId + ".puzzle" + ((patch_Puzzle)(object)puzzle).ExtraFileExtension);
    }

    [MonoModIgnore]
    private extern uint ComputeFileHash(string filePath);

    [MonoModReplace]
    public void UpdateFileHash(Puzzle puzzle) {
        puzzle.SaveToFile(CustomPuzzlePath(puzzle));
        puzzle.fileHash = ComputeFileHash(CustomPuzzlePath(puzzle));
    }
}