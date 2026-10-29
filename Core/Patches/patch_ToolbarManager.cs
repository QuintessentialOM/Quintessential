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
	internal extern void layer_0_method_2047(string param_5645, List<PartTypeForToolbar> partToolbar);

    internal void method_2047(string param_5645, List<PartTypeForToolbar> partToolbar) {
		LocString name;
		if (param_5645 == Translations.Translate("Products")) {
			name = Translations.Translate("Products");
        } else if (param_5645 == Translations.Translate("Reagents")) {
            name = Translations.Translate("Reagents");
        } else if (param_5645 == Translations.Translate("Mechanisms")) {
            name = Translations.Translate("Mechanisms");
        } else if (param_5645 == Translations.Translate("Glyphs")) {
            name = Translations.Translate("Glyphs");
        } else {
			name = Translations.Translate(param_5645);
		}
		AddSection(name, partToolbar);
    }
    public void AddSection(LocString name, List<PartTypeForToolbar> partToolbar) {
		layer_0_method_2047(name, partToolbar);

        foreach (var section in QApi.PanelCustomSections.Where(section => section.Item3 == name.GetEnglish())) {
			List<PartTypeForToolbar> types = [];
			foreach (var type in section.Item1) {
				AddTypeToToolbar(types, type);
			}
            AddSection(section.Item2, types);
		}
    }
}