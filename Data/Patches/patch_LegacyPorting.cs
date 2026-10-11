using MonoMod;
using Quintessential;
using Quintessential.Internal;
using Quintessential.Serialization;
using System.IO;

[MonoModPatch("Quintessential.Internal.LegacyPorting")]
internal static class patch_LegacyPorting {

    [MonoModReplace]
    private static void ExportCampaingNewFormat(string origPath, string exportFolder, Campaign campaign) {
        string relative = Path.GetRelativePath(Path.Combine(QuintessentialLoader.PathLightning, "Mods"), origPath);
        if (Path.IsPathRooted(relative)) relative = Path.GetFileName(relative);
        string newPath = Path.Combine(exportFolder, Path.ChangeExtension(relative, ".jsonc"));
        
        var json = CampaignSerializer.CAMPAIGN.Encode(JsonCodecMap.Instance, campaign);
        if (!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath)); 
        if (File.Exists(newPath)) {
            File.Delete(newPath);
        }
        File.WriteAllText(newPath, json.ToString());

        foreach (var chapter in campaign.chapters) {
            foreach (var item in chapter.campaignItems) {
                if (item.puzzle.HasValue()) {
                    Puzzle puzzle = item.puzzle.GetValue();
                    string path = Path.Combine(Path.GetDirectoryName(newPath), puzzle.puzzleId + ".puzzle.jsonc");

                    var jsonPuzzle = Codecs.PUZZLE.Encode(JsonCodecMap.Instance, puzzle);
                    File.WriteAllText(path, jsonPuzzle.ToString());
                }
            }
        }
    }

    [MonoModReplace]
    private static void ExportJournalNewFormat(string origPath, string exportFolder, Journal journal) {
        string relative = Path.GetRelativePath(Path.Combine(QuintessentialLoader.PathLightning, "Mods"), origPath);
        if (Path.IsPathRooted(relative)) relative = Path.GetFileName(relative);
        string newPath = Path.Combine(exportFolder, Path.ChangeExtension(relative, ".jsonc"));

        var json = CampaignSerializer.JOURNAL.Encode(JsonCodecMap.Instance, journal);
        if (!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));
        if (File.Exists(newPath)) {
            File.Delete(newPath);
        }
        File.WriteAllText(newPath, json.ToString());

        foreach (var volume in journal.volumes) {
            foreach (var puzzle in volume.puzzles) {
                string path = Path.Combine(Path.GetDirectoryName(newPath), puzzle.puzzleId + ".puzzle.jsonc");

                var jsonPuzzle = Codecs.PUZZLE.Encode(JsonCodecMap.Instance, puzzle);
                File.WriteAllText(path, jsonPuzzle.ToString());
            }
        }
    }
}
