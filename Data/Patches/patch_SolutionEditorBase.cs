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
        cursor.EmitLdloc(103);
        cursor.EmitLdelemRef();

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "mors"));
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(3);
        cursor.Index--;
        cursor.EmitDup();
        cursor.Emit(OpCodes.Brtrue, cursor.Next);
        cursor.EmitPop();
        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.Index++;
        cursor.EmitLdelemRef();
        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "vitae"));
        cursor.EmitLdloc(1);
        var newTarget = (Instruction)cursor.Prev;
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(2);
        cursor.Index--;
        cursor.EmitDup();
        cursor.Emit(OpCodes.Brtrue, cursor.Next);
        cursor.EmitPop();
        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.Index++;
        cursor.EmitLdelemRef();
        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.GotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Brfalse_S);
        cursor.Next.Operand = newTarget;

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
        newTarget = (Instruction)cursor.Prev;
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
        cursor.GotoNext(MoveType.After, instr => instr.MatchCall("Molecule", "CreateMonoatomic"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall("Molecule", "CreateMonoatomic"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc(196));
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitNop();
        cursor.Index--;
        cursor.EmitDup();
        cursor.Emit(OpCodes.Brtrue, cursor.Next);
        cursor.EmitPop();
        cursor.Emit(OpCodes.Br, cursor.Next.Next);
        cursor.Index++;
        cursor.EmitLdloc(207);
        cursor.EmitLdcI4(1);
        cursor.EmitAdd();
        cursor.EmitLdelemRef();
        cursor.Emit(OpCodes.Br, cursor.Next.Next.Next.Next);
        cursor.GotoNext(MoveType.After, instr => instr.MatchCall("Molecule", "CreateMonoatomic"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall("Molecule", "CreateMonoatomic"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc(196));
        cursor.RemoveRange(2);
        cursor.EmitLdloc(1);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdloc(210);
        cursor.EmitLdcI4(1);
        cursor.EmitAdd();

        // TODO: unification symbols?
    }

    [MonoModILInject("RenderBoard")]
    static void PatchBoardScale(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method));
        var scale = method.Parameters[^1];
        TypeDefinition color = MonoModRule.Modder.FindType("Color").Resolve();
        MethodDefinition withAlpha = color.Methods.First(f => f.Name.Equals("WithAlpha"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld("ShaderAssets", "board"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdsfld("Color", "White"));
        method.Body.Variables.Add(new VariableDefinition(color));
        var colorVariable = method.Body.Variables[^1];
        cursor.EmitStloc(colorVariable);
        cursor.EmitLdloca(colorVariable);
        cursor.EmitLdarg(scale);
        cursor.EmitCallvirt(withAlpha);

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld("ShaderAssets", "board"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdsfld("Color", "White"));
        method.Body.Variables.Add(new VariableDefinition(color));
        colorVariable = method.Body.Variables[^1];
        cursor.EmitStloc(colorVariable);
        cursor.EmitLdloca(colorVariable);
        cursor.EmitLdarg(scale);
        cursor.EmitCallvirt(withAlpha);
    }
}