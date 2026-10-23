using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System.Linq;

internal class patch_TransmutationEffect {

    [MonoModILInject("RenderTransmEffect")]
    static void PatchAtomRender(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));

        TypeDefinition atomType = MonoModRule.Modder.FindType("Atom").Resolve();
        method.Parameters.Add(new ParameterDefinition("newAtom", ParameterAttributes.None, atomType));
        var origParam = method.Parameters[^1];

        TypeDefinition editor = MonoModRule.Modder.FindType("Editor").Resolve();
        MethodDefinition renderMolecule = editor.Methods.First(m => m.Name.Equals("layer_0_RenderMolecule"));
        ILCursor renderMoleculeCulsor = new(new ILContext(renderMolecule));
        renderMoleculeCulsor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt(method.DeclaringType.Name, method.Name));
        renderMoleculeCulsor.Next.Operand = method;
        renderMoleculeCulsor.EmitLdloc(3);


        MethodReference onCall = MonoModRule.Modder.FindType("Atom").Resolve().Methods.First(f => f.Name.Equals("RenderAtomAlt"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchCall("TextureEffect", "GetCurrentTime"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg(origParam);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.Next.Operand = onCall;
        cursor.EmitLdarg(1);
        cursor.Index++;

        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg(origParam);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.Next.Operand = onCall;
        cursor.EmitLdarg(1);
        cursor.Index++;

        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg2());
        cursor.EmitLdarg(origParam);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.Next.Operand = onCall;
        cursor.EmitLdarg(1);
    }
}
