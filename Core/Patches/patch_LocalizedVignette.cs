using MonoMod;
using Quintessential;
using System.Collections.Generic;
using System.IO;

class patch_LocalizedVignette {

	[MonoModConstructor]
	public void ctor(string vignettePath) {
        LocalizedVignette self = (LocalizedVignette)(object)this;

		self.vignetteDict = [];
		self.vignettePath = vignettePath;
        Language[] languages = {
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
            Language.Czech,
            Language.Polish
        };
        foreach(Language lang in languages) {
            string path1 = Path.Combine("Content", "vignettes", $"{vignettePath}.{Translations.countryCodes[lang]}.txt");

            for(int i = 0; i < QuintessentialLoader.ModContentDirectories.Count && !File.Exists(path1); i++) {
                string content = QuintessentialLoader.ModContentDirectories[i];
                path1 = Path.Combine(content, "Content", "vignettes", $"{vignettePath}.{Translations.countryCodes[Language.English]}.txt");
            }

            string text = File.Exists(path1) ? File.ReadAllText(path1) : "";

            self.vignetteDict[lang] = new Vignette(text, Path.GetFileNameWithoutExtension(path1), lang);
            if(lang == Language.English) {
                Vignette vignette = new(text, Path.GetFileNameWithoutExtension(path1), Language.Pseudo);
                self.vignetteDict[Language.Pseudo] = vignette;
                vignette.data = Translations.ToPseudo(vignette.data);
                foreach(List<VignetteEvent> vignetteEventList in vignette.events) {
                    for(int index = 0; index < vignetteEventList.Count; ++index) {
                        if(vignetteEventList[index].IsLineEvent()) {
                            VignetteEvent.Line lineFields = vignetteEventList[index].ToLineEvent();
                            vignetteEventList[index] = VignetteEvent.CreateLineEvent(lineFields.portrait, Translations.ToPseudo(lineFields.line));
                        }
                    }
                }
            }
        }
    }

    public Document Document;
}
