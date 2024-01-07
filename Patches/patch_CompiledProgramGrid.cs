using MonoMod;
using MonoMod.Utils;

using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

public class patch_CompiledProgramGrid
{
	[MonoModIgnore]
	Dictionary<Part, CompiledProgram> field_2368;

	public extern int orig_method_853(int param_4510);

	public int method_853(int param_4510)
	{
		if (this.field_2368.Count == 0) return 0;

		int num = this.field_2368.Values.First().field_2367.Length;
		if (num == 0) return 0;
		
		return param_4510 % num;
	}
}