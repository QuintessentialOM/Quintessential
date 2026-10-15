using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Collections.Generic;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

class patch_Puzzle : Puzzle {
	
	// Custom puzzle data
	public HashSet<Identifier> CustomPermissions = [];

	public Maybe<PlacedConduit[]> EngineConduits = MaybeHelper.empty;

	[MonoModILInject("SaveToFile")]
    public static void PatchPuzzleIdWrite(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching puzzle ids");
        // Replace "SteamUser.GetSteamID().m_SteamID" with "0" (until a proper format is created)
        if (method.HasBody) {
            ILCursor cursor = new(new ILContext(method));
            if (cursor.TryGotoNext(MoveType.Before,
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

}