#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeModifiers

using MonoMod;
using System.Collections.Generic;

class patch_SolutionEditorBase {
	private static readonly Texture edgeStroke = AssetLoaderHelper.LoadTexture("textures/edge_stroke");

	[MonoModIgnore]
	static extern void RenderHexSelection(IntermediatePartState intermState, IEnumerable<HexIndex> hexes, Color color);

	[MonoModIgnore]
	public extern void orig_RenderGlow(Part part, Vector2 pos, float alpha);

	public void RenderGlow(Part part, Vector2 pos, float alpha) {
		// Automatically render glows for full hex cover parts with no glow texture defined
		if (alpha > 0f) {
			Color color = Color.White.WithAlpha(alpha);
			IntermediatePartState intermState = ((SolutionEditorBase) (object) this).GetIntermState(part, pos);
			var partType = part.GetType();
			if (!partType.IsInputOutput() && !partType.isConduit && !partType.isTrack && partType.isFullHexCover && partType.glowTexture == null) {
				RenderHexSelection(intermState, partType.glyphHexes, color);
			}
		}
		orig_RenderGlow(part, pos, alpha);
	}


	[MonoModIgnore]
	public extern void orig_RenderStroke(Part part, Vector2 pos, float alpha);

	public void RenderStroke(Part part, Vector2 pos, float alpha) {
		// Automatically render strokes for full hex cover parts (including track, conduits, and inputs/outputs) with no stroke texture defined
		if (alpha == 0f) return;

		var partType = part.GetType();

		if (partType.strokeTexture == null && partType.isFullHexCover) {
			var hexes = part.GetHexFootprintWithTransform(((SolutionEditorBase) (object) this).GetSolution(), PartLayer.TopAndBottom, new(), HexRotation.R0);
			var intermState = ((SolutionEditorBase) (object) this).GetIntermState(part, pos);

			var partRenderer = new PartRenderer(intermState.pos, intermState.rotation, new());

			foreach (var hexPos in hexes) {
				foreach (var offset in HexIndex.AdjacentOffsets) {
					if (!hexes.Contains(hexPos + offset)) {
						float angle = HexGrid.standardGrid.ToPixelCoords(offset).Angle();
						partRenderer.RenderOnHexEdge(edgeStroke, hexPos, new Vector2(0f, -1f), new Vector2(-40f, 24f), angle);
					}
				}
			}
		} else {
			orig_RenderStroke(part, pos, alpha);
		}
	}
}
