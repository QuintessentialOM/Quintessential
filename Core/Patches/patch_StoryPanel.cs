using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System.Linq;

internal class patch_StoryPanel {

    [MonoModILInject("RenderVignette")]
    static void PatchPortraitScale(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method));
        MethodDefinition scaled = MonoModRule.Modder.FindType("TextureRenderer").Resolve().Methods.First(m => m.Name.Equals("RenderScaled"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Portrait", "smallTexture"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall("TextureRenderer", "Render"));
        cursor.Next.Operand = scaled;
    }
}
