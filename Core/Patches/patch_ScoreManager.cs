using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_ScoreManager {

	public extern void orig_QuerySteamLeaderboard(string str);
	public void QuerySteamLeaderboard(string str) {
		// no-op
	}

    // removes a steam-related call to upload scores
    [MonoModILInject("SubmitScoreToSteamLeaderboard")]
    public static void PatchScoreManagerSubmit(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching ScoreManager loading");
        if (method.HasBody) {
            ILCursor cursor = new(new ILContext(method));
            if (cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Brfalse_S))
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