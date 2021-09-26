using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;

namespace MonoMod {

	/*[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchGameLogicInit))]
	class PatchGameLogicInit : Attribute { }*/

	[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchSettingsStaticInit))]
	class PatchSettingsStaticInit : Attribute { }

	[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPuzzleIdWrite))]
	class PatchPuzzleIdWrite : Attribute { }

	[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchScoreManagerLoad))]
	class PatchScoreManagerLoad : Attribute { }

	static class MonoModRules {

		static MonoModRules() {
			MonoModRule.Modder.Log("Patching OM");
		}

		public static void PatchSettingsStaticInit(MethodDefinition method, CustomAttribute attrib) {
			MonoModRule.Modder.Log("Patching settings static init");
			// Set "class_110.field_1012" (Steam support) to false
			if(method.HasBody) {
				ILCursor cursor = new ILCursor(new ILContext(method));
				if(cursor.TryGotoNext(MoveType.Before,
										instr => instr.MatchLdcI4(1),
										instr => instr.MatchStsfld("class_110", "field_1012"))) {
					cursor.Remove();
					cursor.Emit(OpCodes.Ldc_I4_0);
				} else {
					Console.WriteLine("Failed to disable Steam setting in class_110!");
					throw new Exception();
				}
			} else {
				Console.WriteLine("Failed to disable Steam setting in class_110!");
				throw new Exception();
			}
		}

		public static void PatchPuzzleIdWrite(MethodDefinition method, CustomAttribute attrib) {
			MonoModRule.Modder.Log("Patching puzzle ids");
			// Replace "SteamUser.GetSteamID().m_SteamID" with "0" (until a proper format is created)
			if(method.HasBody) {
				ILCursor cursor = new ILCursor(new ILContext(method));
				if(cursor.TryGotoNext(MoveType.Before,
										instr => instr.MatchCall("Steamworks.SteamUser", "GetSteamID"),
										instr => instr.MatchLdfld("Steamworks.CSteamID", "m_SteamID"))) {
					cursor.Remove();
					cursor.Remove();
					cursor.Emit(OpCodes.Ldc_I8, (long)0);
				}
			} else {
				Console.WriteLine("Failed to modify puzzle serialization!");
				throw new Exception();
			}
		}

		public static void PatchScoreManagerLoad(MethodDefinition method, CustomAttribute attrib) {
			MonoModRule.Modder.Log("Patching ScoreManager loading");
			if(method.HasBody) {
				ILCursor cursor = new ILCursor(new ILContext(method));
				if(cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Brfalse_S))
					&& cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Brfalse_S))
					&& cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Brfalse_S))) {
					cursor.Emit(OpCodes.Ret);
				} else {
					Console.WriteLine("Failed to modify ScoreManager loading (no match)!");
					throw new Exception();
				}
			} else {
				Console.WriteLine("Failed to modify ScoreManager loading (no body)!");
				throw new Exception();
			}
		}
	}
}