using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Internal;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
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

    private static readonly Dictionary<IReadOnlyList<BondType>, Tuple<Texture, Index2, Texture, bool>> RenderedBondTextures = new(new BondTypesComparer());
    private static readonly RenderTargetHandle BondRenderTarget = new();
    private static readonly RenderTargetHandle BondNormalMapRenderTarget = new();


    [MonoModIgnore] // # Unsafe to use, assumes no mod patches RenderMolecule before QuintData.
    public static extern void layer_0_RenderBond(patch_Bond bond, Vector2 offset, HexIndex hexOffset, float rotationAngle, float opacityMultiplier, float height, SolutionEditorBase solutionEditor);
    [MonoModReplace]
    public static void RenderBond(patch_Bond bond, Vector2 offset, HexIndex hexOffset, float rotationAngle, float opacityMultiplier, float height, SolutionEditorBase solutionEditor) {
        if (!RenderedBondTextures.TryGetValue(bond.GetBondTypes(), out Tuple<Texture, Index2, Texture, bool> textures)) {
            var textureSizes = bond.GetBondTypes().Select(t => t.bondTexture.texture.size);
            Index2 textureSize = new(textureSizes.Max(vec => vec.X), textureSizes.Max(vec => vec.Y));
            BondRenderTarget.targetSize = textureSize;
            using (class_226.method_596(BondRenderTarget.GetTarget())) {
                class_226.method_600(Color.Transparent);
                foreach (var bondType in bond.GetBondTypes()) {
                    TextureRenderer.Render(bondType.bondTexture.texture, (textureSize.ToVector2() - bondType.bondTexture.texture.size.ToVector2()) / 2f);
                }
            }
            BondNormalMapRenderTarget.targetSize = textureSize;
            bool noNormals = true;
            using (class_226.method_596(BondNormalMapRenderTarget.GetTarget())) {
                class_226.method_600(Color.Black);
                foreach (var bondType in bond.GetBondTypes()) {
                    TextureRenderer.Render(bondType.bondTexture.normalMap, (textureSize.ToVector2() - bondType.bondTexture.normalMap.size.ToVector2()) / 2f);
                    noNormals &= bondType.bondTexture.normalMap == Assets.textures.black || bondType.bondTexture.normalMap == Assets.textures.transparent;
                }
            }
            //Logger.LogNoTime(Codecs.LIST_ID.Encode(JsonCodecMap.Instance, [.. bond.GetBondTypes().Select(bondType => bondType.Id)]).ToJsonString());

            textures = new(BondRenderTarget.GetTarget().renderedTexture, textureSize, BondNormalMapRenderTarget.GetTarget().renderedTexture, noNormals);
            BondRenderTarget.GetTarget().renderedTexture = Renderer.GetEmptyTexture(textureSize.X, textureSize.Y);
            BondNormalMapRenderTarget.GetTarget().renderedTexture = Renderer.GetEmptyTexture(textureSize.X, textureSize.Y);
            RenderedBondTextures.Add([.. bond.GetBondTypes()], textures);
        }
        
        Vector2 pos1 = offset + HexGrid.standardGrid.ToPixelCoords(bond.hexPos1 - hexOffset).Rotated(rotationAngle);
        Vector2 pos2 = offset + HexGrid.standardGrid.ToPixelCoords(bond.hexPos2 - hexOffset).Rotated(rotationAngle);
        float angle = (pos2 - pos1).Angle();
        Vector2 center = Utils.InterpolateVect(pos1, pos2, 0.5f);
        Vector2 halfTextureSize = 0.5f * textures.Item2.ToVector2();
        Matrix4 transformation = Matrix4.GetTranslation(center.ToVector3(0f)) * Matrix4.RotXY(angle) * Matrix4.GetTranslation(-halfTextureSize.ToVector3(0f)) * Matrix4.GetScale(textures.Item2.ToVector3(0f));
        Color color = new(height, opacityMultiplier, 0f, 1f);
        TextureRenderer.GetBatcher().shader = Assets.shaderAssets.bond;
        TextureRenderer.GetBatcher().textures[1] = textures.Item3;
        TextureRenderer.GetBatcher().texCoords.X = Utils.Modulo(textures.Item4 ? 0 : angle, 6.2831855f);
        TextureRenderer.Render(textures.Item1, color, transformation);
        TextureRenderer.GetBatcher().shader = TextureRenderer.GetBatcher().defaultSpriteShader;
        TextureRenderer.GetBatcher().textures[1] = Assets.textures.white;
        TextureRenderer.GetBatcher().texCoords.X = 0f;

        foreach (var bondType in bond.GetBondTypes()) {
            if (solutionEditor != null) {
                foreach (BondEffect bondEffect in bond.effects) {
                    bondEffect.RenderEffect(solutionEditor, center, angle);
                }
                bond.effects.RemoveAll(effect => effect.IsComplete(solutionEditor));
            }
        }
    }
}
[MonoModPatch("Editor")]
public static class patch_Editor2 {
    public static void RenderBond(patch_Bond bond, Vector2 offset, HexIndex hexOffset, float rotationAngle, float opacityMultiplier, float height, SolutionEditorBase solutionEditor) {
        bond.RenderBond(offset, hexOffset, rotationAngle, opacityMultiplier, height, solutionEditor);
    }
}
