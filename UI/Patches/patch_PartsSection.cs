#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeModifiers

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Linq;

[MonoModPatch("SolutionEditorPartsPanel/PartsSection")]
internal class patch_PartsSection {
    [MonoModILInject(".ctor")]
    static void GeneratePartIcons(MethodDefinition method, CustomAttribute attribute) {
        MethodDefinition generatePartIconIfNecessary = method.DeclaringType.Methods.First(f => f.Name.Equals("GeneratePartIconIfNecessary"));

        ILCursor cursor = new(new ILContext(method)); // Create cursor

        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc(4)); // inside of `foreach (item in partTypes)`
        cursor.EmitLdarg3(); // SolutionEditorScreen
        cursor.EmitLdloc(4); // PartTypeForToolbar
        cursor.EmitCall(generatePartIconIfNecessary);
    }

    private static void GeneratePartIconIfNecessary(SolutionEditorScreen solEditScrn, PartTypeForToolbar part) {
        if (!part.type.IsInputOutput() && part.type.baseTexture == null && part.type.hoverTexture == null && part.type.isFullHexCover && part.type.glyphHexes.Length > 0) {
            var partType = (patch_PartType) (object) part.type;
            partType.generatedIconBaseRenderT ??= new();
            partType.generatedIconHoverRenderT ??= new();
            var partTypeForToolbar = (patch_PartTypeForToolbar) (object) part;
            partTypeForToolbar.generatedIconBaseRenderT = RenderPartIcon(solEditScrn, part.type, false);
            partTypeForToolbar.generatedIconHoverRenderT = RenderPartIcon(solEditScrn, part.type, true);
        }
    }

    [MonoModILInject("method_2052")]
    static void UseGeneratedPartIcons(MethodDefinition method, CustomAttribute attribute) {
        var generatedIconBaseRenderT = MonoModRule.Modder.FindType("PartTypeForToolbar").Resolve().Fields.First(f => f.Name.Equals("generatedIconBaseRenderT"));
        var generatedIconHoverRenderT = MonoModRule.Modder.FindType("PartTypeForToolbar").Resolve().Fields.First(f => f.Name.Equals("generatedIconHoverRenderT"));

        var getTarget = MonoModRule.Modder.FindType("RenderTargetHandle").Resolve().Methods.First(f => f.Name.Equals("GetTarget"));
        var renderedTexture = MonoModRule.Modder.FindType("RenderTarget").Resolve().Fields.First(f => f.Name.Equals("renderedTexture"));


        // Patch to use generated part icons if they're present
        new ILContext(method).Invoke(context => {
            ILCursor cursor = new(context); // Create cursor

            var afterIfElse = cursor.DefineLabel();
            var elseBranch = cursor.DefineLabel();

            var afterTernary = cursor.DefineLabel();
            var ternaryElseBranch = cursor.DefineLabel();

            // Target before `Texture obj2 = (isHover ? type.hoverTexture : type.baseTexture);` - it will go in the `else` branch
            cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld("PartType", "baseTexture"));
            cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdloc(12));
            // Any branches to here should also run our code
            cursor.MoveAfterLabels();

            // `if (part.generatedIconBaseRenderT != null)`
            cursor.EmitLdloc(9); // PartTypeForToolbar
            cursor.EmitLdfld(generatedIconBaseRenderT);
            cursor.EmitBrfalse(elseBranch);

            // (isHover
            cursor.EmitLdloc(12);
            cursor.EmitBrfalse(ternaryElseBranch);
            // ? part.generatedIconHoverRenderT
            cursor.EmitLdloc(9); // PartTypeForToolbar
            cursor.EmitLdfld(generatedIconHoverRenderT);
            cursor.EmitBr(afterTernary);
            // : part.generatedIconBaseRenderT)
            cursor.MarkLabel(ternaryElseBranch);
            cursor.EmitLdloc(9); // PartTypeForToolbar
            cursor.EmitLdfld(generatedIconBaseRenderT);

            cursor.MarkLabel(afterTernary);
            // .GetTarget().renderedTexture
            cursor.EmitCallvirt(getTarget);
            cursor.EmitLdfld(renderedTexture);
            cursor.EmitBr(afterIfElse);
            cursor.MarkLabel(elseBranch);

            cursor.GotoNext(MoveType.Before, instr => instr.MatchDup());
            cursor.MarkLabel(afterIfElse);
        });
    }


    [MonoModILInject("method_2052")]
    static void PatchMoleculeDisplayName(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method)); // Create cursor
        MethodDefinition getDisplayName = MonoModRule.Modder.FindType("Molecule").Resolve().Methods.First(f => f.Name.Equals("GetDisplayName"));

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda("Molecule", "displayName"))){
            cursor.Prev.OpCode = OpCodes.Callvirt;
            cursor.Prev.Operand = getDisplayName;

            method.Body.Variables.Add(new VariableDefinition(getDisplayName.ReturnType));
            var maybeVar = method.Body.Variables[^1];
            cursor.EmitStloc(maybeVar);
            cursor.EmitLdloca(maybeVar);
        }
    }

	// Based on Editor.RenderMoleculeForDisplay. Generates a part icon for the part tray.
	internal static RenderTargetHandle RenderPartIcon(SolutionEditorScreen solEditScrn, PartType partType, bool isHover) {
		RenderTargetHandle renderTargetHandle = isHover ? ((patch_PartType) (object) partType).generatedIconHoverRenderT : ((patch_PartType) (object) partType).generatedIconBaseRenderT;
		Bounds2 partBounds = Bounds2.Undefined;
		foreach (HexIndex hexPos in partType.glyphHexes) {
			Bounds2 hexBounds = Bounds2.CenteredOn(HexGrid.standardGrid.ToPixelCoords(hexPos, Vector2.Zero), HexGrid.standardGrid.hexSize.X, HexGrid.standardGrid.hexSize.Y * 1.3f);
			partBounds = partBounds.UnionedWith(hexBounds);
		}
        // max size determined based on vanilla part icons - different than molecules because parts are in two columns
        Vector2 scaleBound = new(114f, 114f);
		float scaleFactor = (partType.glyphHexes.Length == 1) ? 1f : Math.Min(Math.Min(scaleBound.X / partBounds.Width, scaleBound.Y / partBounds.Height), 0.7f);
		int margin = 20;
		Index2 targetSize = (partBounds.Size * scaleFactor).CeilingToInt() + new Index2(margin * 2, margin * 2);
		Vector2 partOffset = targetSize.ToVector2() / 2 / scaleFactor - partBounds.Center;
		renderTargetHandle.targetSize = targetSize;
		RenderTarget target = renderTargetHandle.GetTarget(out var wasCreated);
		if (wasCreated) {
			using (class_226.method_597(target, Matrix4.GetScale(scaleFactor))) {
				class_226.method_600(Color.Transparent);
				Texture shadowOrGlow = isHover ? Assets.textures.parts.icons.input_output_hover : Assets.textures.parts.icons.input_output_shadow;
				foreach (HexIndex hexPos in partType.glyphHexes) {
					Vector2 vector2 = HexGrid.standardGrid.ToPixelCoords(hexPos, Vector2.Zero);
					TextureRenderer.Render(shadowOrGlow, (vector2 - shadowOrGlow.size.ToVector2() / 2 + partOffset).Rounded());
				}
				var part = new Part(partType, false);
				solEditScrn.RenderPart(part, partOffset, false, false, false);
				return renderTargetHandle;
			}
		}
		return renderTargetHandle;
	}
}
