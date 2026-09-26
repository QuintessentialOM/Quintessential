using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System.Linq;

public static class patch_Editor {

    [MonoModIgnore] // # Unsafe to use, assumes no mod patches RenderMolecule before QuintData.
    public static extern void layer_0_RenderMolecule(patch_Molecule molecule, Vector2 offset, HexIndex hexPos, float rotationAngle, float opacityMultiplier, float height, float shadowStrength, bool isOutputRender, SolutionEditorBase solutionEditor);
    public static void RenderMolecule(Molecule molecule, Vector2 offset, HexIndex hexPos, float rotationAngle, float opacityMultiplier, float height, float shadowStrength, bool isOutputRender, SolutionEditorBase solutionEditor) {
        ((patch_Molecule)(object)molecule).RenderMolecule(offset, hexPos, rotationAngle, opacityMultiplier, height, shadowStrength, isOutputRender, solutionEditor);
    }

    [MonoModILInject("RenderMolecule")]
    static void PatchRenderMolecule(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference onCall = MonoModRule.Modder.FindType("Atom").Resolve().Methods.First(f => f.Name.Equals("RenderAtom"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderAtom"));
        cursor.Next.Operand = onCall;
        cursor.EmitLdarg(8);
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdfld("Atom","atomType"));
        cursor.Remove();

        onCall = MonoModRule.Modder.FindType("Bond").Resolve().Methods.First(f => f.Name.Equals("RenderBond"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("Editor", "RenderBond"));
        cursor.Next.Operand = onCall;
    }
}
