#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class patch_Translations {
    private static Dictionary<string, LocString> translationDict;

    public static extern void orig_Init();
    public static void Init() {
        orig_Init();

        // Loading localisation data into LocalizationLayer.GlobalLayer
        foreach (var dir in QuintessentialLoader.ModContentDirectories) {
            var langDirPath = Path.Combine(dir, "Content", "lang");
            if (Directory.Exists(langDirPath)) {
                foreach (var file in Directory.GetFiles(langDirPath)) {
                    var Language = Translations.countryCodes.FirstOrDefault(
                        code => Path.GetFileName(file) == code.Value + ".jsonc",
                        new(0, null));

                    if (Language.Value != null) {
                        LocalizationLayer.CurrentFileLanguage = Language.Key;
                        DataSerializer.Deserialize<LocalizationLayer>(file);
                    }
                }
            }
        }
        LocalizationLayer.GlobalLayer.AddSelfAndSubToDictionary(translationDict);
    }

    public static extern LocString orig_Translate(string toTranslate);
    public static LocString Translate(string toTranslate) {
        var orig = orig_Translate(toTranslate);
        ((patch_LocString)(object)orig).Key = toTranslate;
        return orig;
    }

    public static extern LocString orig_WithAllLanguages(string toTranslate);
    public static LocString WithAllLanguages(string toTranslate) {
        var orig = orig_WithAllLanguages(toTranslate);
        ((patch_LocString)(object)orig).Key = "<" + toTranslate + ">";
        return orig;
    }

    public static LocString TranslateByType(string toTranslate) {
        if (string.IsNullOrEmpty(toTranslate)) return WithAllLanguages("");
        if (toTranslate.StartsWith('<') && toTranslate.EndsWith('>'))
            return WithAllLanguages(toTranslate[1..^1]);
        return Translate(toTranslate);
    }


    [MonoModILInject("Translate")]
    public static void DeformatKeysPatch(MethodDefinition method, CustomAttribute attrib) {

        MonoModRule.Modder.Log("Patching Translations translate");

        if (!method.HasBody) {
            throw new Exception("Unable to patch translate. (no body)");
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdsfld("AppConsts", "useEnglishWhenMissing"),
            instr => instr.OpCode == OpCodes.Brtrue_S,
            instr => instr.MatchLdsfld("Translations", "missingTranslationReplacement")
        )) {
            throw new Exception("Unable to patch translate. (no call)");
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("Translations").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "Deformat" && f.IsStatic);

        while (cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg0()
            )) {

            cursor.Emit(OpCodes.Callvirt, call);
        }
    }

    public static string Deformat(string original) {
        if (original.Contains(' ') || original != original.ToLower()) return original; // skip vanilla english keys
        return original.EscapeFormatting();
    }
}
