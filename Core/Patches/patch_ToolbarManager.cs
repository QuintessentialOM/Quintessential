using MonoMod;
using MonoMod.Utils;
using Quintessential;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006 // Naming Styles

[MonoModPatch("SolutionEditorPartsPanel/ToolbarManager")]
class patch_ToolbarManager {

	// add our parts to the panel
	[MonoModIgnore]
	SolutionEditorPartsPanel partsPanel;
	
	public extern void orig_AddTypeToToolbar(List<PartTypeForToolbar> partToolbar, PartType partType);
	[MonoModPublic]
	public void AddTypeToToolbar(List<PartTypeForToolbar> partToolbar, PartType partType) {
		// find the puzzle we're in
		DynamicData selfData = new(partsPanel);
		var sol = selfData.Get<SolutionEditorScreen>("solEditScrn");
		Puzzle puzzle = sol.GetSolution().GetPuzzle();
		// check if we have the appropriate custom permissions
		var perms = ((patch_Puzzle)(object)puzzle).CustomPermissions ??= [];
		var checker = ((patch_PartType)(object)partType).CustomPermissionCheck;

		if(checker == null || checker(perms))
            orig_AddTypeToToolbar(partToolbar, partType);

        foreach (var pair in QApi.PanelParts.Where(pair => partType.Equals(pair.Item2)))
            AddTypeToToolbar(partToolbar, pair.Item1);
    }

	[MonoModIgnore] // # unsafe
	internal extern void layer_0_AddSection(string param_5645, List<PartTypeForToolbar> partToolbar);

    internal void AddSection(string name, List<PartTypeForToolbar> partToolbar) {
		LocString localizedName;
		if (name == Translations.Translate("Products")) {
            localizedName = Translations.Translate("Products");
        } else if (name == Translations.Translate("Reagents")) {
            localizedName = Translations.Translate("Reagents");
        } else if (name == Translations.Translate("Mechanisms")) {
            localizedName = Translations.Translate("Mechanisms");
        } else if (name == Translations.Translate("Glyphs")) {
            localizedName = Translations.Translate("Glyphs");
        } else {
            localizedName = Translations.Translate(name);
		}
		AddSection(localizedName, partToolbar);
    }
    public void AddSection(LocString name, List<PartTypeForToolbar> partToolbar) {
        layer_0_AddSection(name, partToolbar);

        foreach (var section in QApi.PanelCustomSections.Where(section => section.Item3 == name.GetEnglish())) {
			List<PartTypeForToolbar> types = [];
			foreach (var type in section.Item1) {
				AddTypeToToolbar(types, type);
			}
            AddSection(section.Item2, types);
		}
    }
}