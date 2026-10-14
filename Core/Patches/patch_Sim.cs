using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_Sim{

	// Make important fields public
	[MonoModPublic]
	public SolutionEditorBase solutionEditor;
	[MonoModPublic]
	public Dictionary<Part, PartSimState> simulationDict;
	[MonoModPublic]
	public Dictionary<Part, Sim.HolderMovementInfo> holderInfos;
	[MonoModPublic]
	public List<Molecule> molecules;
	[MonoModPublic]
	public List<Sim.Collider> additionalCollisions;

    [MonoModPublic]
    [MonoModIgnore]
    private extern Maybe<AtomReference> GetAtomReference(Part part, HexIndex offset, List<Part> holdingParts, bool allowPartAttachedAtoms);

    // Hold onto held grippers
    public List<Part> HeldGrippers;

	// Helper methods to find held or unheld atoms
	public Maybe<AtomReference> FindAtomRelative(Part part, HexIndex offset){
		return FindAtom(part.InFrontBy(offset));
	}
	
	public Maybe<AtomReference> FindAtom(HexIndex position){
		var simStates = simulationDict;
		foreach(Molecule molecule in molecules) {
			if(molecule.GetAtoms().TryGetValue(position, out Atom atom)){
				bool isHeld = HeldGrippers != null && HeldGrippers.Any(part => simStates[part].newPosition == position);
				return new AtomReference(molecule, position, atom.atomType, atom, isHeld);
			}
		}

		return MaybeHelper.empty;
	}

    // Run custom behaviours
    public extern void orig_RunCycleGlyphs(bool isCycleStart);
	public void RunCycleGlyphs(bool isCycleStart) {
		// fill the list of grippers
		List<Part> allParts = solutionEditor.GetSolution().parts;
		Dictionary<Part, PartSimState> simStates = simulationDict;
		HeldGrippers = new();
		foreach(var part in allParts)
			foreach(var gripper in part.subparts)
				if(simStates[gripper].heldMolecule.HasValue())
					HeldGrippers.Add(gripper);
        // run the cycle
        orig_RunCycleGlyphs(isCycleStart);

        // and then process things that happen after
		foreach(var action in QApi.ToRunAfterCycle)
			action((Sim)(object)this, isCycleStart);
	}

    [MonoModILInject("RunCycleGlyphs")]
    static void PatchWasActivated(MethodDefinition method, CustomAttribute attribute) {

        if (!method.HasBody) {
            throw new Exception("Unable to patch wasActivated reset. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        TypeDefinition recipeType = MonoModRule.Modder.FindType("PartSimState").Resolve();
        FieldDefinition recipesField = recipeType.Fields.First(f => f.Name.Equals("wasActivated"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc(7));
        cursor.EmitLdloc(7);
        cursor.EmitLdcI4(0);
        cursor.EmitStfld(recipesField);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("PartSimState", "isProcessing"), instr => instr.OpCode == OpCodes.Brtrue)) {
            var toEdit = cursor.Prev;
            cursor.Goto((Instruction)cursor.Prev.Operand, MoveType.Before);
            if (cursor.Previous.OpCode != OpCodes.Br) continue;
            var exitTarget = (Instruction)cursor.Prev.Operand;
            cursor.EmitLdloc(7);
            var newTarget = cursor.Prev;
            cursor.EmitLdfld(recipesField);
            cursor.Emit(OpCodes.Brtrue, exitTarget);
            cursor.EmitLdloc(7);
            cursor.EmitLdcI4(1);
            cursor.EmitStfld(recipesField);
            cursor.Goto(toEdit, MoveType.Before);
            cursor.Next.Operand = newTarget;
        }

        cursor.Index = 0;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "PlaySound"))) {
            if (cursor.Previous.Previous.MatchLdfld("SoundAssets", "solution")) continue;
            cursor.EmitLdloc(7);
            cursor.EmitLdcI4(1);
            cursor.EmitStfld(recipesField);
        }
    }
}