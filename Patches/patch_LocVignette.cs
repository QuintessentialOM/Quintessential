using MonoMod;
using Quintessential;
using System.Collections.Generic;
using System.IO;

[MonoModPatch("class_264")]
class patch_LocVignette {

	[MonoModConstructor]
	public void ctor(string key) {
		class_264 self = (class_264)(object)this;

		self.field_2091 = new Dictionary<Language, Vignette>();
		self.field_2090 = key;
        Language[] languages = new Language[12]
        {
            Language.English,
            Language.German,
            Language.French,
            Language.Russian,
            Language.Chinese,
            Language.Japanese,
            Language.Spanish,
            Language.Korean,
            Language.Turkish,
            Language.Ukrainian,
            Language.Portuguese,
            Language.Czech
        };
        foreach(Language lang in languages) {
            string path1 = Path.Combine("Content", "vignettes", string.Format("{0}.{1}.txt", key, class_134.field_1498[lang]));

            for(int i = 0; i < QuintessentialLoader.ModContentDirectories.Count && !File.Exists(path1); i++) {
                string content = QuintessentialLoader.ModContentDirectories[i];
                path1 = Path.Combine(content, "Content", "vignettes", string.Format("{0}.{1}.txt", key, class_134.field_1498[Language.English]));
            }

            string text = File.Exists(path1) ? File.ReadAllText(path1) : "";

            self.field_2091[lang] = new Vignette(text, Path.GetFileNameWithoutExtension(path1), lang);
            if(lang == Language.English) {
                Vignette vignette = new Vignette(text, Path.GetFileNameWithoutExtension(path1), Language.Pseudo);
                self.field_2091[Language.Pseudo] = vignette;
                vignette.field_4124 = class_134.method_249(vignette.field_4124);
                foreach(List<VignetteEvent> vignetteEventList in vignette.field_4125) {
                    for(int index = 0; index < vignetteEventList.Count; ++index) {
                        if(vignetteEventList[index].method_2215()) {
                            VignetteEvent.LineFields lineFields = vignetteEventList[index].method_2218();
                            vignetteEventList[index] = VignetteEvent.method_2212(lineFields.field_4136, class_134.method_249(lineFields.field_4093));
                        }
                    }
                }
            }
        }
    }
}
