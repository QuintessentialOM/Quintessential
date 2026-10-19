using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

internal class patch_Vignette {

    public string backgroundPath;
    public string cinematicTitle;

    [MonoModILInject(".ctor")]
    static void PatchBackground(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition handleBackground = method.DeclaringType.Methods.First(m => m.Name.Equals("ParseBackground"));
        MethodDefinition handleTitle = method.DeclaringType.Methods.First(m => m.Name.Equals("ParseTitle"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdstr("Portraits"));
        MethodReference compare = cursor.Next.Next.Operand as MethodReference;
        Instruction start = cursor.Prev.Previous;

        cursor.GotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Leave);
        Instruction end = cursor.Next.Operand as Instruction;

        cursor.Goto(start);
        cursor.EmitLdloc(8);
        start = cursor.Prev;
        cursor.EmitLdstr("Background");
        cursor.EmitCall(compare);
        cursor.Emit(OpCodes.Brfalse_S, cursor.Next);
        cursor.EmitLdarg(0);
        cursor.EmitLdloc(9);
        cursor.EmitCall(handleBackground);
        cursor.Emit(OpCodes.Br, end);

        cursor.Goto(start);
        cursor.EmitLdloc(8);
        start = cursor.Prev;
        cursor.EmitLdstr("Title");
        cursor.EmitCall(compare);
        cursor.Emit(OpCodes.Brfalse_S, cursor.Next);
        cursor.EmitLdarg(0);
        cursor.EmitLdloc(9);
        cursor.EmitCall(handleTitle);
        cursor.Emit(OpCodes.Br, end);
    }

    void ParseBackground(string parsed) {
        backgroundPath = parsed;
    }
    void ParseTitle(string parsed) {
        cinematicTitle = parsed;
    }
}
