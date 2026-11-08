using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using Quintessential;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

abstract class patch_SolutionEditorBase : SolutionEditorBase
{

    // renders parts
    // adds support for custom part renderers

    public extern void orig_RenderPartBase(Part part, Vector2 pos);
    public void RenderPartBase(Part part, Vector2 pos)
    {
        orig_RenderPartBase(part, pos);
        IntermediatePartState intermediateState = GetIntermState(part, pos);
        PartRenderer renderer = new(intermediateState.pos, intermediateState.rotation, Editor.GetLightSourcePosition());
        foreach (var r in QApi.PartRenderers)
            if (r.Item1(part))
                r.Item2(part, pos, this, renderer);
    }

    [MonoModILInject("RenderPart")]
    static void PatchRenderPart(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition getProgramindexPos = method.DeclaringType.Methods.First(m => m.Name.Equals("GetPartProgramindexRenderPos"));
        MethodDefinition allowProgramindex = method.DeclaringType.Methods.First(m => m.Name.Equals("AllowedPartProgramindexRender"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt("SolutionEditorBase", "RenderPartOverlays"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg2());
        cursor.EmitLdarg1();
        cursor.Index++;
        cursor.EmitCall(getProgramindexPos);
        cursor.EmitLdarg1();
        cursor.Index++;
        cursor.EmitCall(allowProgramindex);
    }
    static Vector2 GetPartProgramindexRenderPos(Part part, Vector2 pos) {
        return pos + ((patch_PartType)(object)part.GetType()).programIndexOffset;
    }
    static bool AllowedPartProgramindexRender(Part part, bool allowed) {
        if (((patch_PartType)(object)part.GetType()).showProgramIndex != null) return ((patch_PartType)(object)part.GetType()).showProgramIndex.Value;
        return allowed && !part.GetType().isFullHexCover;
    }
}