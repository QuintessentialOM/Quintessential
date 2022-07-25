using MonoMod;
using MonoMod.Utils;

using Quintessential;
using System.Collections.Generic;

using PartType = class_139;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006 // Naming Styles

[MonoModPatch("SolutionEditorPartsPanel/class_428")]
class patch_SolutionEditorPartsPanelSection {

	// add our parts to the panel
	[MonoModIgnore]
	SolutionEditorPartsPanel field_3972;
	
	public extern void orig_method_2046(List<PartTypeForToolbar> parts, PartType type);
	public void method_2046(List<PartTypeForToolbar> parts, PartType type) {
		// find the puzzle we're in
		DynamicData selfData = new(field_3972);
		var sol = selfData.Get<SolutionEditorScreen>("field_2007");
		Puzzle puzzle = sol.method_502().method_1934();
		// check if we have the appropriate custom permissions
		var perms = ((patch_Puzzle)(object)puzzle).CustomPermissions;
		var checker = ((patch_PartType)(object)type).CustomPermissionCheck;

		if(checker == null || checker(perms))
			orig_method_2046(parts, type);
		
		foreach(var pair in QApi.PanelParts)
			if(type.Equals(pair.Right))
				method_2046(parts, pair.Left);
	}
}