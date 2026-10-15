using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

internal class patch_DocumentScreen {
    [MonoModIgnore]
    private readonly LocalizedVignette vignette;
    [MonoModIgnore]
    private readonly string[] sections;

    [MonoModILInject("RenderFrame")]
    static void PatchBackground(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition tryRender = method.DeclaringType.Methods.First(m => m.Name.Equals("TryRenderDocument"));

        Instruction start = cursor.Next;
        cursor.Index = cursor.Instrs.Count - 1;
        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchCall("InputManager", "IsClickPressed"));
        Instruction after = cursor.Prev;
        cursor.Goto(start);

        cursor.EmitLdarg0();
        cursor.EmitCall(tryRender);
        cursor.Emit(OpCodes.Brtrue, after);
    }

    public bool TryRenderDocument() {
        if (((patch_LocalizedVignette)(object)vignette).Document == null) 
            return false;
        ((patch_LocalizedVignette)(object)vignette).Document.Render(InputManager.screenSize / 2f, sections);
        return true;
    }
}
