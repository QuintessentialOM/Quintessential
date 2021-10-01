using MonoMod;
using Quintessential;
using System.Collections.Generic;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("SolutionEditorPartsPanel/class_428")]
class patch_SolutionEditorPartsPanelSection {

	// add our parts to the panel

	public extern void orig_method_2046(List<PartTypeForToolbar> param_5643, class_139 param_5644);
	public void method_2046(List<PartTypeForToolbar> param_5643, class_139 param_5644) {
		orig_method_2046(param_5643, param_5644);
		foreach(var pair in QApi.PanelParts)
			if(param_5644.Equals(pair.Right))
				method_2046(param_5643, pair.Left);
	}
}