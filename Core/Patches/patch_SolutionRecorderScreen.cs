#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential.Internal;
using System;
using System.Linq;

[MonoModPatch("SolutionRecorderScreen")]
class patch_SolutionRecorderScreen {

	private static void MarkOnFrame(){
		var markerPos = new Vector2(826 - 60 - 40, 647 - 61);
		var verPos = new Vector2(826 - 60 - 40 - 20, 647 - 40);
        TextureRenderer.Render(Assets.textures.atoms.elements.quintessence_symbol, markerPos);
        TextureRenderer.RenderText(QuintessentialCore.Instance.Meta.Version.ToString(), verPos, Assets.fonts.crimson_16_5, Color.LightGray, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), Assets.textures.white, int.MaxValue, true, true);
    }

    [MonoModILInject("RenderFrame")]
    static void PatchGifRecorderFrame(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching GIF recorder frame rendering");
        if (method.HasBody) {
            ILCursor cursor = new(new ILContext(method));
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("TextureRenderer", "Render"))) {

                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("TextureRenderer", "Render"))) {
                    TypeDefinition holder = MonoModRule.Modder.FindType("SolutionRecorderScreen").Resolve();
                    MethodDefinition to = holder.Methods.First(m => m.Name.Equals("MarkOnFrame"));
                    cursor.Emit(OpCodes.Call, to);
                    return;
                }
            }
            Console.WriteLine("Failed to modify GIF recorder frame rendering (no match)!");
            throw new Exception();
        }
        Console.WriteLine("Failed to modify GIF recorder frame rendering (no body)!");
        throw new Exception();
    }
}