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

            CampaignSerializer.CurrentCampaignPath = Path.GetDirectoryName(path);
            Campaign c = CampaignSerializer.CAMPAIGN.Decode(JsonCodecMap.Instance, JsonNode.Parse(file));
            CampaignSerializer.CurrentCampaignPath = "";
            return c;
        }
        throw new Exception("Invalid puzzle format: " + path);
    }

    //public static List<JournalVolume> LoadJournal(string path) {
    //    throw new NotImplementedException("Exporting a journal to the new format requeres Quintessential Data!");
    //}
}
