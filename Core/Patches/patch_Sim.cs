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
		// fill the colliders of grippers
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

    [MonoModIgnore] private float simDeltaTime;
    [MonoModIgnore] private extern void MarkHexesAsOccupied(Vector2 centerPos);
    [MonoModIgnore] public extern Solution GetSolution();
    [MonoModIgnore] public extern int GetCycle();
    [MonoModIgnore] private extern void orig_SimulateCycleMovements();
    [MonoModIgnore] private static readonly float collSizeSubpart;
    [MonoModIgnore] private static readonly float collSizeChamber;
    [MonoModIgnore] private static readonly float collSizeAtom;
    [MonoModIgnore] private static readonly LocString simErrorAtomColl;
    [MonoModIgnore] private static readonly LocString simErrorArmColl;
    [MonoModIgnore] private static readonly LocString simErrorAToAColl;
    [MonoModIgnore] private static readonly LocString simErrorAtomLeave;
    [MonoModIgnore] private static readonly LocString simErrorArmLeave;
    private void SimulateCycleMovements() {
        if (QuintessentialCoreSettings.Instance.DisableCollisionOptimization)
            orig_SimulateCycleMovements();
        else SimulateCycleMovementsOptimised();
    }
    private void SimulateCycleMovementsOptimised() {
        Time startTime = Time.Now();
        solutionEditor.ClearDebugMarkers();
        for (float time = simDeltaTime; time <= 1f; time += simDeltaTime) {
            List<Sim.Collider> colliders = [];
            List<ColliderRegions> regions = [];
            foreach (Molecule molecule in solutionEditor.GetSimulation().GetMolecules()) {
                int listCount = colliders.Count;
                Vector2 vector;
                HexIndex newPosition;
                float num2;
                if (solutionEditor.GetHolder(molecule).GetOrDefault(out Part part)) {
                    PartSimState simState = solutionEditor.GetSimulation().GetSimState(part);
                    IntermediatePartState intermState = solutionEditor.GetIntermState(part, Vector2.Zero, time);
                    vector = intermState.pos;
                    newPosition = simState.newPosition;
                    num2 = intermState.leftoverDeltaPivot;
                } else {
                    vector = Vector2.Zero;
                    newPosition = new HexIndex(0, 0);
                    num2 = 0f;
                }
                float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, minX = float.PositiveInfinity, minY = float.PositiveInfinity;
                foreach (KeyValuePair<HexIndex, Atom> keyValuePair in molecule.GetAtoms()) {
                    HexIndex key = keyValuePair.Key;
                    Vector2 vector2 = vector + HexGrid.standardGrid.ToPixelCoords(key - newPosition).Rotated(num2);
                    maxX = Math.Max(maxX, vector2.X);
                    maxY = Math.Max(maxY, vector2.Y);
                    minX = Math.Min(minX, vector2.X);
                    minY = Math.Min(minY, vector2.Y);
                    var newCollider = new Sim.Collider(Sim.ColliderType.Atom, vector2);
                    foreach (var region in regions) {
                        if (region.bounds.Expanded(newCollider.radius, newCollider.radius).Contains(newCollider.center)) {
                            for (int i = region.firstIndex; i < region.firstIndex + region.colliderCount; i++) {
                                Sim.Collider collider2 = colliders[i];
                                if (Vector2.Distance(newCollider.center, collider2.center) < newCollider.radius + collider2.radius) {
                                    HandleCollision(newCollider, collider2, time);
                                }
                            }
                        }
                    }
                    colliders.Add(newCollider);
                }
                regions.Add(new(Bounds2.WithCorners(minX - collSizeAtom, minY - collSizeAtom, maxX + collSizeAtom, maxY + collSizeAtom), listCount, colliders.Count - listCount));
                //solutionEditor.AddDebugMarker(regions[^1].bounds.BottomLeft);
                //solutionEditor.AddDebugMarker(regions[^1].bounds.BottomRight);
                //solutionEditor.AddDebugMarker(regions[^1].bounds.TopLeft);
                //solutionEditor.AddDebugMarker(regions[^1].bounds.TopRight);
            }
            int moleculeCounts = colliders.Count;
            foreach (Part part2 in solutionEditor.GetSolution().CollectPartsAndSubparts().Where(part => part.HasAtoms())) {
                IntermediatePartState intermState2 = solutionEditor.GetIntermState(part2, Vector2.Zero, time);
                foreach (HexIndex hexIndex in part2.GetType().attachedAtoms.Keys) {
                    Vector2 vector3 = HexGrid.standardGrid.ToPixelCoords(hexIndex);
                    Vector2 vector4 = intermState2.pos + vector3.Rotated(intermState2.rotation);
                    colliders.Add(new Sim.Collider(Sim.ColliderType.Atom, vector4));
                }
            }
            foreach (Part part3 in solutionEditor.GetSolution().parts.Where(part => part.HasArms() || part.HasAtoms())) {
                IntermediatePartState intermediatePartState = solutionEditor.GetIntermState(part3, Vector2.Zero, time);
                colliders.Add(new Sim.Collider(Sim.ColliderType.PartBase, intermediatePartState.pos));
                foreach (Part part4 in part3.subparts) {
                    intermediatePartState = solutionEditor.GetIntermState(part4, Vector2.Zero, time);
                    colliders.Add(new Sim.Collider {
                        type = Sim.ColliderType.PartSubpart,
                        center = intermediatePartState.pos,
                        radius = collSizeSubpart
                    });
                }
            }
            if (GetSolution().GetPuzzle().productionInfo.GetOrDefault(out ProductionInfo productionInfo)) {
                foreach (PlacedChamber placedChamber in productionInfo.chambers) {
                    foreach (HexIndex hexIndex2 in placedChamber.chamber.borderHexes) {
                        HexIndex hexIndex3 = placedChamber.hexPos + hexIndex2;
                        colliders.Add(new Sim.Collider {
                            type = Sim.ColliderType.Chamber,
                            center = HexGrid.standardGrid.ToPixelCoords(hexIndex3),
                            radius = collSizeChamber
                        });
                    }
                }
            }
            colliders.AddRange(additionalCollisions);
            regions.Add(new(Bounds2.WithCorners(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity), moleculeCounts, colliders.Count - moleculeCounts));
            for (int k = regions[^1].firstIndex; k < colliders.Count; k++) {
                Sim.Collider collider = colliders[k];
                foreach (var region in regions) {
                    if (region.bounds.Expanded(new Vector2(collider.radius, collider.radius)).Contains(collider.center)) {
                        int colliderIndex = Math.Min(region.firstIndex + region.colliderCount, k);
                        for (int l = region.firstIndex; l < colliderIndex; l++) {
                            Sim.Collider collider2 = colliders[l];
                            if (Vector2.Distance(collider.center, collider2.center) < collider.radius + collider2.radius) {
                                HandleCollision(collider, collider2, time);
                            }
                        }
                    }
                }
                MarkHexesAsOccupied(collider.center);
            }
        }
        additionalCollisions.Clear();
        Log.LogData("atom collision detection startTime (ms)", Time.MillisecondsSince(startTime).ToString("0.00"));
    }

    public void HandleCollision(Sim.Collider collider1, Sim.Collider collider2, float time) {
        bool flag = collider1.type == Sim.ColliderType.Atom || collider2.type == Sim.ColliderType.Atom;
        bool flag2 = collider1.type == Sim.ColliderType.PartBase || collider2.type == Sim.ColliderType.PartBase;
        bool flag3 = collider1.type == Sim.ColliderType.PartSubpart || collider2.type == Sim.ColliderType.PartSubpart;
        bool flag4 = collider1.type == Sim.ColliderType.Chamber || collider2.type == Sim.ColliderType.Chamber;
        bool flag5 = true;
        string text;
        if (collider1.type == Sim.ColliderType.Atom && collider2.type == Sim.ColliderType.Atom) {
            text = simErrorAtomColl;
        } else if (collider1.type == Sim.ColliderType.PartBase && collider2.type == Sim.ColliderType.PartBase) {
            text = simErrorArmColl;
        } else if (flag && flag2) {
            text = simErrorAToAColl;
        } else if (flag4 && flag) {
            text = simErrorAtomLeave;
        } else if (flag4 && flag3) {
            text = simErrorArmLeave;
        } else {
            flag5 = false;
            text = null;
        }
        if (flag5) {
            float num3 = Utils.Clamp(time, 0f, 0.999f);
            List<Vector2> list2 = [];
            if (collider1.type != Sim.ColliderType.Chamber) {
                list2.Add(collider1.center);
            }
            if (collider2.type != Sim.ColliderType.Chamber) {
                list2.Add(collider2.center);
            }
            solutionEditor.HandleSimulationError(num3, text, [.. list2]);
        }
    }

    public struct ColliderRegions {
        public Bounds2 bounds;
        public int firstIndex, colliderCount;
        public ColliderRegions(Bounds2 bounds, int firstIndex, int colliderCount) {
            this.bounds = bounds;
            this.firstIndex = firstIndex;
            this.colliderCount = colliderCount;
        }
    }
}