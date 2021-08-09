using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
class patch_ScoreManager {

	[PatchScoreManagerLoad]
	[MonoModIgnore]
	public extern void method_1373(Puzzle param_5140, enum_127 param_5141, int param_5142);

	public extern void orig_method_1374(string str);
	public void method_1374(string str) {
		// no-op
	}
}