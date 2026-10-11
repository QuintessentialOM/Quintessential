using MonoMod;
using Quintessential.Internal;
using Quintessential.Serialization;
using System;
using System.IO;
using System.Text.Json.Nodes;

[MonoModPatch("Quintessential.Internal.DataLoader")]
internal static class patch_DataLoader {

    [MonoModIgnore]
    public static extern Texture GetIcon(string path);
    [MonoModIgnore]
    public static extern Texture GetOther(string path);

    public static string GetTexturePath(Texture texture, string @default) {
        string path = texture?.sourceFile.GetOrDefault(@default) ?? @default;
        return string.IsNullOrEmpty(path) ? path : Path.GetRelativePath("Content", path);
    }

    [MonoModReplace]
    public static Campaign LoadCampaign(string path) {
        if (Path.GetExtension(path) == ".json" || Path.GetExtension(path) == ".jsonc") {
            string file = File.ReadAllText(path);
            if (Path.GetExtension(path) == ".jsonc") file = DataSerializer.JsoncToJson(file);

            CampaignSerializer.CurrentPuzzleFolder = Path.GetDirectoryName(path);
            Campaign c = CampaignSerializer.CAMPAIGN.Decode(JsonCodecMap.Instance, JsonNode.Parse(file));
            CampaignSerializer.CurrentPuzzleFolder = "";
            return c;
        }
        throw new Exception("Invalid Campaign format: " + path);
    }

    [MonoModReplace]
    public static Journal LoadJournal(string path) {
        if (Path.GetExtension(path) == ".json" || Path.GetExtension(path) == ".jsonc") {
            string file = File.ReadAllText(path);
            if (Path.GetExtension(path) == ".jsonc") file = DataSerializer.JsoncToJson(file);

            CampaignSerializer.CurrentPuzzleFolder = Path.GetDirectoryName(path);
            Journal j = CampaignSerializer.JOURNAL.Decode(JsonCodecMap.Instance, JsonNode.Parse(file));
            CampaignSerializer.CurrentPuzzleFolder = "";
            return j;
        }
        throw new Exception("Invalid Journal format: " + path);
    }
}
