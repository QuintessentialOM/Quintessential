using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
class patch_Puzzle {

	// sets your puzzle ID to 0
	// will be improved in the future

	[PatchPuzzleIdWrite]
	[MonoModIgnore]
	public static extern void method_1248(string param_4980);
}