using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
class patch_ScoreManager {

	[PatchScoreManagerLoad]
	[MonoModIgnore]
	public extern void method_1369(Puzzle param_5132, enum_133 param_5133, int param_5134);

	public extern void orig_method_1370(string str);
	public void method_1370(string str) {
		// no-op
	}
}