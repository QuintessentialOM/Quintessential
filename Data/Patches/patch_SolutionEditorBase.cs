using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Linq;

class patch_SolutionEditorBase {

    [MonoModILInject("RenderPartBase")]
    static void PatchRecipeSystem(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Recipe System init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Recipe System init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        TypeDefinition recipeType = MonoModRule.Modder.FindType("PartSimState").Resolve();
        FieldDefinition processingAtoms = recipeType.Fields.First(f => f.Name.Equals("processingAtoms"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("PartTypes", "animismusGlyph"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "salt"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdloc(104);
        cursor.EmitLdelemRef();
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdsfld("AtomTypes", "mors"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "mors"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(3);
        cursor.EmitLdelemRef();
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "vitae"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        var newTarget = (Instruction)cursor.Prev; 
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(2);
        cursor.EmitLdelemRef();
        cursor.GotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Brfalse_S);
        cursor.Next.Operand = newTarget;

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("PartTypes", "projectionGlyph"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quicksilver"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(0);
        cursor.EmitLdelemRef();

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("PartTypes", "proliferationGlyph"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quicksilver"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(1);
        cursor.EmitLdelemRef();


        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("PartTypes", "dispersionGlyph"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quintessence"));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(0);
        cursor.EmitLdelemRef();
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdloc(197));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdloc(197));
        cursor.Remove();
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(1);
        cursor.Index++;
        cursor.EmitAdd();

        // TODO: unification symbols?
    }
}