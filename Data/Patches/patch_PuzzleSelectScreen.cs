using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_PuzzleSelectScreen {

    #region ComponentCalls

    internal static bool OnCampaignItemSelect(patch_CampaignItem item) {
        bool handleAsVanilla = true;
        item.OnSelected(ref handleAsVanilla);
        return handleAsVanilla;
    }

    [MonoModILInject("RenderChapter")]
    static void PatchSaveSnapshot(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnCampaignItemSelect"));
        cursor.GotoNext(instr => instr.MatchCallvirt("GameLogic", "PushScreen"));
        cursor.GotoPrev(instr => instr.MatchLdfld("CampaignItem", "itemType"));
        cursor.GotoPrev(MoveType.After, instr => instr.MatchLdloc(10));
        FieldReference campaignItem = cursor.Next.Operand as FieldReference;
        cursor.GotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue);
        Instruction end = cursor.Previous.Operand as Instruction;
        cursor.EmitLdloc(10);
        cursor.EmitLdfld(campaignItem);
        cursor.EmitCall(onCall);
        cursor.Emit(OpCodes.Brfalse, end);
    }

    #endregion
}