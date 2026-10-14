using Quintessential.Serialization;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Quintessential;

// TODO rename to LocalizationLayer for consistency
[JsonConverter(typeof(LocalisationLayerJsonConverter))]
public class LocalisationLayer {
    public static readonly LocalisationLayer GlobalLayer = new();
    public static Language CurrentFileLanguage;

    public Dictionary<Language, string> locDictionary = [];
    public Dictionary<string, LocalisationLayer> subLayers = [];

    public void AddSelfAndSubToDictionary(Dictionary<string, LocString> translationDict, string key = "") {
        bool useEnglishAsReplacement = !AppConsts.isDevEnv;

        if (key != "") {
            var loc = new LocString {
                locDictionary = locDictionary
            };

            if (!loc.locDictionary.TryGetValue(Language.English, out string value)) {
                loc.locDictionary[Language.English] = key.EscapeFormatting();
                loc.locDictionary[Language.Pseudo] = key.EscapeFormatting();
            } else {
                loc.locDictionary[Language.Pseudo] = Translations.ToPseudo(value);
            }
            foreach (var lang in Translations.countryCodes.Keys) {
                if (!loc.locDictionary.ContainsKey(lang)) {
                    loc.locDictionary[lang] = useEnglishAsReplacement ? loc.locDictionary[Language.English] : key.EscapeFormatting();
                }
            }

            translationDict.Add(key, loc);

            key += ".";
        }
        foreach (var layer in subLayers) {
            layer.Value.AddSelfAndSubToDictionary(translationDict, key + layer.Key);
        }
    }
}
