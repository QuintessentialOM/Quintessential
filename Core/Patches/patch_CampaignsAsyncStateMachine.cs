using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;

[MonoModPatch("Campaigns/AsyncStateMachine")]
internal class patch_CampaignsAsyncStateMachine {


    [MonoModILInject("MoveNext")]
    static void CustomUnlockRequirements(MethodDefinition method, CustomAttribute attrib) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition handleCustom = method.DeclaringType.Methods.First(m => m.Name.Equals("HandleCustomUnlockRequirements"));
        
        cursor.GotoNext(MoveType.After, instr => instr.MatchThrow());
        Instruction target = cursor.Next;
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdstr("Unhandled case"));

        cursor.EmitLdloca(3);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(method.DeclaringType.Fields.First(f => f.Name.Equals("currentChapter")));
        cursor.EmitLdarg0();
        cursor.EmitLdfld(method.DeclaringType.Fields.First(f => f.Name.Equals("currentCampaignItem")));
        cursor.EmitCall(handleCustom);
        cursor.Emit(OpCodes.Brtrue, target);
    }

    public static bool HandleCustomUnlockRequirements(ref bool isSatisfied, CampaignChapter chapter, CampaignItem item) {
        if (item.requirement is UnlockReqNever) {
            isSatisfied = false;
            return true;
        } else if (item.requirement is UnlockReqCustom custom) {
            isSatisfied = custom.check();
            return true;
        } else if (item.requirement is UnlockReqCompleteMany many) {
            isSatisfied = many.requiredItemIds.All(id => {
                Maybe<CampaignItem> maybe = patch_CampaignsForAsync.FindCampaignItemById(id);
                return maybe.HasValue() && maybe.GetValue().IsComplete();
            });
            return true;
        } else if (item.requirement is UnlockReqCompleteSomeOfMany someOfMany) {
            int completed = someOfMany.requiredItemIds.Count(id => {
                Maybe<CampaignItem> maybe = patch_CampaignsForAsync.FindCampaignItemById(id);
                return maybe.HasValue() && maybe.GetValue().IsComplete();
            });
            isSatisfied = completed > someOfMany.required;
            return true;
        } else if (item.requirement is UnlockReqCompleteSomeOfChapter someOfChapter) {
            int completed = chapter.campaignItems.Count(item => item.IsComplete());
            isSatisfied = completed > someOfChapter.required;
            return true;
        }
        return false;
    }

}

[MonoModIgnore]
[MonoModPatch("Campaigns")]
internal class patch_CampaignsForAsync {

    [MonoModIgnore]
    public static extern Maybe<CampaignItem> FindCampaignItemById(string itemId);
}