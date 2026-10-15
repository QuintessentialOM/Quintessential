using MonoMod;
using System;
using System.Linq;

class patch_LocString : LocString {
    public string Key = null;
    private Func<string> AutoTranslate;
    public static patch_LocString Format(LocString format, params LocString[] args) {
        // !//TODO Set the Key!!!
        return new patch_LocString() {
            AutoTranslate = () => {
                return string.Format(format.GetTranslated(), [..args.Select(loc => loc.GetTranslated())]);
            }
        };
    }
    [MonoModReplace]
    public virtual string GetTranslated() {
        if (AutoTranslate != null) return AutoTranslate();
        return locDictionary[Translations.currentLanguage];
    }
}
