using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;

[MonoModPatch("Campaigns/AsyncStateMachine_EnumeratePuzzleSelectEvents")]
internal class patch_CampaignsAsyncStateMachine {


    [MonoModILInject("MoveNext")]
    static void CustomUnlockRequirements(MethodDefinition method, CustomAttribute attrib) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition handleCustom = method.DeclaringType.Methods.First(m => m.Name.Equals("HandleCustomUnlockRequirements"));
        MethodDefinition handleChapter = method.DeclaringType.Methods.First(m => m.Name.Equals("HandleChapterRequirements"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchThrow());
        Instruction target = cursor.Next;
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdstr("Unhandled case"));

        cursor.EmitLdarg0();
        cursor.EmitLdloca(3);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(method.DeclaringType.Fields.First(f => f.Name.Equals("currentChapter")));
        cursor.EmitLdarg0();
        cursor.EmitLdfld(method.DeclaringType.Fields.First(f => f.Name.Equals("currentCampaignItem")));
        cursor.EmitCall(handleCustom);
        cursor.Emit(OpCodes.Brtrue, target);

        // & Add checks for Chapter unlocks
        cursor.Index = 0;
        cursor.GotoNext(MoveType.After, instr => instr.MatchCall(out var m) && m.Name.Equals("Last"));
        Instruction end = cursor.Next.Operand as Instruction;
        cursor.Index -= 6;
        cursor.EmitNop();
        Instruction start = cursor.Next;

        Instruction newAfter = cursor.Previous;
        Instruction oldAfter = start;
        foreach (var instr in cursor.Instrs) {
            if (ReferenceEquals(instr.Operand, oldAfter))
                instr.Operand = newAfter;
        }
        foreach (var handler in method.Body.ExceptionHandlers) {
            if (ReferenceEquals(handler.TryEnd, oldAfter))
                handler.TryEnd = newAfter;
            if (ReferenceEquals(handler.TryStart, oldAfter))
                handler.TryStart = newAfter;
            if (ReferenceEquals(handler.FilterStart, oldAfter))
                handler.FilterStart = newAfter;
            if (ReferenceEquals(handler.HandlerStart, oldAfter))
                handler.HandlerStart = newAfter;
            if (ReferenceEquals(handler.HandlerEnd, oldAfter))
                handler.HandlerEnd = newAfter;
        }
        cursor.Goto(end);
        int endIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(endIndex - cursor.Index);

        cursor.Index = 0;
        cursor.GotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld(out var fd) && fd.Name.Equals("chapterIndex"),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld(out var fd) && fd.Name.Equals("chapters"),
            instr => instr.MatchLdlen(),
            instr => instr.MatchConvI4(),
            instr => instr.OpCode == OpCodes.Blt || instr.OpCode == OpCodes.Blt_S
        );
        FieldReference chapterIndex = cursor.Next.Next.Operand as FieldReference;
        start = cursor.Next;
        cursor.Index += 7;
        target = cursor.Prev.Operand as Instruction;
        cursor.EmitNop();
        cursor.Index--;
        cursor.Previous.Operand = cursor.Next;

        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.Index++;

        cursor.EmitLdarg0();
        cursor.EmitCall(handleChapter);
        cursor.Emit(OpCodes.Brtrue, target);

        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(chapterIndex);
        cursor.EmitLdcI4(1);
        cursor.EmitAdd();
        cursor.EmitStfld(chapterIndex);
        cursor.Emit(OpCodes.Br, start);
    }

    public bool HandleCustomUnlockRequirements(ref bool isSatisfied, CampaignChapter chapter, CampaignItem item) {
        if (item.requirement is UnlockReqNever) {
            isSatisfied = false;
            return true;
        } else if (item.requirement is UnlockReqCustom custom) {
            isSatisfied = custom.check(chapters, chapterIndex, chapter, item);
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
            isSatisfied = completed >= someOfMany.required;
            return true;
        }
        return false;
    }

    public bool HandleChapterRequirements() {
        patch_CampaignChapter thisChapter = chapters[chapterIndex];
        if (thisChapter.requirement is null) {
            return chapterIndex == 0 || chapters[chapterIndex - 1].campaignItems.Last().IsComplete();
        } else if (thisChapter.requirement is UnlockReqNothing) {
            return true;
        } else if (thisChapter.requirement is UnlockReqNever) {
            return false;
        } else if (thisChapter.requirement is UnlockReqCustom custom) {
            return custom.check(chapters, chapterIndex, thisChapter, null);

        } else if (thisChapter.requirement is UnlockReqCompleteCampaignItem single) {
            Maybe<CampaignItem> maybe = patch_CampaignsForAsync.FindCampaignItemById(single.requiredItemId);
            return maybe.HasValue() && maybe.GetValue().IsComplete();

        } else if (thisChapter.requirement is UnlockReqPuzzlesComplete puzzlesComplete) {
            if (chapterIndex == 0)
                throw new OpusMagnumException("UnlockReqPuzzlesComplete requirement is invalid for first campaign chapter.");
            int completedPuzzles = chapters[chapterIndex - 1].campaignItems.Count(item => item.itemType == CampaignItemType.Puzzle && item.IsComplete());
            return completedPuzzles >= puzzlesComplete.requiredMinimum;

        } else if (thisChapter.requirement is UnlockReqCompleteMany many) {
            return many.requiredItemIds.All(id => {
                Maybe<CampaignItem> maybe = patch_CampaignsForAsync.FindCampaignItemById(id);
                return maybe.HasValue() && maybe.GetValue().IsComplete();
            });

        } else if (thisChapter.requirement is UnlockReqCompleteSomeOfMany someOfMany) {
            int completed = someOfMany.requiredItemIds.Count(id => {
                Maybe<CampaignItem> maybe = patch_CampaignsForAsync.FindCampaignItemById(id);
                return maybe.HasValue() && maybe.GetValue().IsComplete();
            });
            return completed >= someOfMany.required;

        }
        throw new OpusMagnumException("Invalid unlock requirement type for campaign chapter: " + thisChapter.requirement.GetType().Name);
    }
    [MonoModIgnore] private patch_CampaignChapter[] chapters;
    [MonoModIgnore] private int chapterIndex;
}

[MonoModIgnore]
[MonoModPatch("Campaigns")]
internal class patch_CampaignsForAsync {

    [MonoModIgnore]
    public static extern Maybe<CampaignItem> FindCampaignItemById(string itemId);
}