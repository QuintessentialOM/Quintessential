using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

class patch_PuzzleEditorScreen {
    [MonoModIgnore]
    private Maybe<Puzzle> selectedPuzzle;

    [MonoModILInject("RenderFrame")]
    public static void PatchMoleculeEditorScreenMoleculeError(MethodDefinition method, CustomAttribute attrib) {
        ILCursor cursor = new(new ILContext(method));
        cursor.GotoNext(instr => instr.MatchLdstr("Upload"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdloc(25));

        cursor.EmitLdarg0();
        cursor.EmitCall(method.DeclaringType.Methods.First(m => m.Name.Equals("IsVanillaPuzzle")));
        cursor.EmitAnd();
    }

    public bool IsVanillaPuzzle() {
        return selectedPuzzle.HasValue() && ((patch_Puzzle)(object)selectedPuzzle.GetValue()).ExtraFileExtension == "";
    }
}