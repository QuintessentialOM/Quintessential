using System.Collections.Generic;
using System.Linq;
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
	
	// Hold onto held grippers
	public List<Part> HeldGrippers;

	// Helper methods to find held or unheld atoms
	public Maybe<AtomReference> FindAtomRelative(Part part, HexIndex offset){
		return FindAtom(part.method_1184(offset));
	}
	
	public Maybe<AtomReference> FindAtom(HexIndex position){
		var simStates = field_3821;
		foreach(Molecule molecule in field_3823){
			if(molecule.method_1100().TryGetValue(position, out Atom atom)){
				bool isHeld = HeldGrippers != null && HeldGrippers.Any(part => simStates[part].field_2724 == position);
				return new AtomReference(molecule, position, atom.field_2275, atom, isHeld);
			}
		}

		return struct_18.field_1431;
	}

	// Run custom behaviours
	public extern void orig_method_1832(bool first);
	public void method_1832(bool first){
		// fill the list of grippers
		List<Part> allParts = field_3818.method_502().field_3919;
		Dictionary<Part, PartSimState> simStates = field_3821;
		HeldGrippers = new();
		foreach(var part in allParts)
			foreach(var gripper in part.field_2696)
				if(simStates[gripper].field_2729.method_1085())
					HeldGrippers.Add(gripper);
		// run the cycle
		orig_method_1832(first);
		// and then process things that happen after
		foreach(var action in QApi.ToRunAfterCycle)
			action((Sim)(object)this, first);
	}
}