using MonoMod;

using PartType = class_139;

class patch_Part{
	
	// this part type
	[MonoModIgnore]
	public extern PartType method_1159();
	// this IO index
	[MonoModIgnore]
	public extern int method_1167();
	// setter for output amount
	[MonoModIgnore]
	private extern void method_1170(int param_2840);
	
	// handle output count overrides
	public extern void orig_method_1176(Solution solution, int param_4911);

	public void method_1176(Solution solution, int param_4911){
		orig_method_1176(solution, param_4911);
		
		bool isPolymer = this.method_1159().field_1554;
		if(!isPolymer){
			PuzzleInputOutput[] list = (!method_1159().field_1541 ? solution.method_1934().field_2771 : solution.method_1934().field_2770);
			if(list == null || list.Length <= method_1167())
				return;

			PuzzleInputOutput io = list[method_1167()];
			if(io == null)
				return;

			int amount = ((patch_PuzzleInputOutput)(object)io).AmountOverride;
			if(amount > 0)
				method_1170(amount);
		}
	}
}