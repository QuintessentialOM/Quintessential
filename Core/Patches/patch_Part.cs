#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Linq;

class patch_Part {
	// this part type
	[MonoModIgnore]
	public extern PartType GetType();
	// this IO index
	[MonoModIgnore]
	public extern int GetInputOutputIndex();
	// setter for output amount
	[MonoModIgnore]
	private extern void SetOutputCount(int requiredCount);
	
	// handle output count overrides
	public extern void orig_SetupInputOutputFromSolution(Solution solution, int inputOutputIndex);

	public void SetupInputOutputFromSolution(Solution solution, int inputOutputIndex) {
        orig_SetupInputOutputFromSolution(solution, inputOutputIndex);
		
		bool isPolymer = this.GetType().isRepOutput;
		if(!isPolymer){
			PuzzleInputOutput[] list = (!GetType().isInput ? solution.GetPuzzle().outputs : solution.GetPuzzle().inputs);
			if(list == null || list.Length <= GetInputOutputIndex())
				return;

			PuzzleInputOutput io = list[GetInputOutputIndex()];
			if(io == null)
				return;

			int amount = ((patch_PuzzleInputOutput)(object)io).AmountOverride;
			if(amount > 0)
                SetOutputCount(amount);
		}
	}

    [MonoModReplace]
	public bool IsNotConduit()
    {
        PartType t = GetType();
        return !(t.isConduit || ((patch_PartType)(object)t).IsForced);
    }

	[MonoModILInject("ResetTypeById")]
	public static void PatchResetById(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition getById = MonoModRule.Modder.FindType("PartTypes").Resolve().Methods.First(f => f.FullName.Equals("Maybe`1<PartType> PartTypes::GetById(Quintessential.Identifier)"));
        MethodDefinition getId = MonoModRule.Modder.FindType("PartType").Resolve().Methods.First(f => f.Name.Equals("get_Id"));
		cursor.GotoNext(instr => instr.MatchLdfld("PartType", "id"));
		cursor.RemoveRange(2);
		cursor.EmitCall(getId);
        cursor.EmitCall(getById);
    }
}