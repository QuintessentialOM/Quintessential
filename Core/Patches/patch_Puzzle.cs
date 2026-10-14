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
	
	// Is modded content allowed in this puzzle?
	// Controls whether this is saved to/from a vanilla `.puzzle` file, or a Quintessential `.puzzle.yaml` file
	// Don't set this if you don't know what you're doing!
	//public bool IsModdedPuzzle = false;

	public Maybe<PlacedConduit[]> EngineConduits = MaybeHelper.empty;

	//public extern void orig_SaveToFile(string path);
	//public void SaveToFile(string path) {
	//	if (IsModdedPuzzle)
	//		File.WriteAllText(path, YamlHelper.Serializer.Serialize(PuzzleModel.FromPuzzle((Puzzle)(object)this)));
	//	else
	//		orig_SaveToFile(path);
	//}

	//public static extern Puzzle orig_LoadFromFile(string path);
	//public static Puzzle LoadFromFile(string path) {
	//	if (Path.GetExtension(path) == ".yaml") {
	//		Puzzle p = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(path)));
	//		((patch_Puzzle)(object)p).IsModdedPuzzle = true;
	//		return p;
	//	}
	//	return orig_LoadFromFile(path);
	//}

	//public void ConvertFormat(bool modded) {
	//	Puzzle self = (Puzzle)(object)this;
	//	WorkshopManager wm = GameLogic.instance.workshopManager;
	//	// delete
	//	File.Delete(((patch_WorkshopManager)(object)wm).CustomPuzzlePath(self));
	//	// update
	//	IsModdedPuzzle = modded;
	//	// save
	//	self.SaveToFile(((patch_WorkshopManager)(object)wm).CustomPuzzlePath(self));
	//	//wm.RegenPuzzleId(self);
	//}

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