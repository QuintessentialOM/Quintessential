using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;

[MonoModPatch("OSInfo")]
internal class patch_OSInfo {

    [MonoModILInject("GetSavePath")]
    private static void PatchSetSaveFolder(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdstr("AlternateSavePath"));
        cursor.EmitLdstr("Modded");
        cursor.EmitStloc0();
    }
}
