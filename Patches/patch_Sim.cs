using System.Collections.Generic;
using MonoMod;
using Quintessential;

#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_Sim{

	// Make important fields public
	[MonoModPublic]
	public SolutionEditorBase field_3818;
	[MonoModPublic]
	public Dictionary<Part, PartSimState> field_3821;
	[MonoModPublic]
	public Dictionary<Part, Sim.class_401> field_3822;
	[MonoModPublic]
	public List<Molecule> field_3823;
	[MonoModPublic]
	public List<Sim.struct_122> field_3826;
	
	// Run custom behaviours
	public extern void orig_method_1832(bool first);
	public void method_1832(bool first){
		orig_method_1832(first);
		foreach(var action in QApi.ToRunAfterCycle)
			action((Sim)(object)this, first);
	}
}