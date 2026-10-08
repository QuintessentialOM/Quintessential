using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;

class patch_MoleculeEditorScreen {

    [MonoModILInject("MoleculeError")]
    public static void PatchMoleculeEditorScreenMoleculeError(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching molecule editor screen error detector");
        if (!method.HasBody) {
            Console.WriteLine("Failed to modify molecule editor error detector (no body)!");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method)); // Create cursor

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "molecule",
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.MatchStloc(1),
            instr => instr.OpCode == OpCodes.Br
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no bond checker)!");
            throw new Exception();
        }
        Instruction start = cursor.Previous;
        cursor.Goto((Instruction)cursor.Previous.Operand);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdloc(1),
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Brtrue,
            instr => instr.OpCode == OpCodes.Leave
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no last leave)");
            throw new Exception();
        }
        Instruction end = cursor.Previous;
        int endIndex = cursor.Index - 1;
        // Immediately leaves the loop
        start.Operand = end;
        cursor.Goto(start);
        cursor.RemoveRange(endIndex - cursor.Index);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "molecule",
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.MatchStloc(1),
            instr => instr.OpCode == OpCodes.Br
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (second error)");
            throw new Exception();
        }
        Instruction start2 = cursor.Previous;
        cursor.Goto((Instruction)cursor.Previous.Operand);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Brtrue,
            instr => instr.OpCode == OpCodes.Leave_S
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no second last leave)");
            throw new Exception();
        }
        Instruction end2 = cursor.Previous;
        int endIndex2 = cursor.Index - 1;
        // Immediately leaves the loop
        //start2.Operand = end2;
        cursor.Goto(start2);
        cursor.RemoveRange(endIndex2 - cursor.Index);
    }
}