using Mono.Cecil;
using MonoMod;
using Quintessential;
using System.Collections.Generic;

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
}