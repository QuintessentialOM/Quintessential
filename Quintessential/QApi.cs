using System;
using System.Collections.Generic;

namespace Quintessential {

	using PartType = class_139;
	using RenderHelper = class_195;
	using PartTypes = class_191;

	public class QApi {

		public static readonly List<Pair<Predicate<Part>, PartRenderer>> PartRenderers = new List<Pair<Predicate<Part>, PartRenderer>>();
		public static readonly List<Pair<PartType, PartType>> PanelParts = new List<Pair<PartType, PartType>>();

		public static void Init() {
			
		}

		/// <summary>
		/// Adds a part type to the end of a part panel section, making it accessible for placement.
		/// This does not allow for adding inputs or outputs.
		/// </summary>
		/// <param name="type">The part type to be added.</param>
		/// <param name="mechanism">Whether to add to the mechanisms section or the glyphs section.</param>
		public static void AddPartTypeToPanel(PartType type, bool mechanism) {
			if(mechanism)
				AddPartTypeToPanel(type, PartTypes.field_1771);
			else
				AddPartTypeToPanel(type, PartTypes.field_1782);
		}

		/// <summary>
		/// Adds a part type to the part panel after another given type, making it accessible for placement.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="after"></param>
		public static void AddPartTypeToPanel(PartType type, PartType after) {
			if(type == null || after == null) 
				Logger.Log("Tried to add a null part to the parts panel, or tried to add a part after a null part, not adding.");
			else if(type.Equals(after))
				Logger.Log("Tried to add a part to the part panel after itself (circular reference), not adding.");
			else
				PanelParts.Add(new Pair<PartType, PartType>(type, after));
		}

		/// <summary>
		/// Adds a PartRenderer, which renders any parts that satisfy the given predicate. Usually, this predicate simply checks the part type of the part.
		/// </summary>
		/// <param name="renderer">The PartRenderer to be added and displayed.</param>
		/// <param name="typeChecker">A predicate that determines which parts the renderer should try to display.</param>
		public static void AddPartTypesRenderer(PartRenderer renderer, Predicate<Part> typeChecker) {
			PartRenderers.Add(new Pair<Predicate<Part>, PartRenderer>(typeChecker, renderer));
		}

		/// <summary>
		/// Adds a part type to the list of all part types.
		/// </summary>
		/// <param name="type">The part type to be added.</param>
		public static void AddPartType(PartType type) {
			Array.Resize(ref PartTypes.field_1785, PartTypes.field_1785.Length + 1);
			PartTypes.field_1785[PartTypes.field_1785.Length - 1] = type;
		}

		/// <summary>
		/// Adds a part type, adding it to the list of part types and adding a renderer for that part type.
		/// </summary>
		/// <param name="type">The part type to be added.</param>
		/// <param name="renderer">A PartRenderer to render instances of that part type.</param>
		public static void AddPartType(PartType type, PartRenderer renderer) {
			AddPartType(type);
			AddPartTypesRenderer(renderer, part => part.method_1159() == type);
		}
	}

	/// <summary>
	/// A function that renders a part.
	/// </summary>
	/// <param name="part">The part to be displayed.</param>
	/// <param name="position">The position of the part.</param>
	/// <param name="editor">The solution editor that the part is being displayed in.</param>
	/// <param name="renderer">An object containing functions for rendering images, at different positions/rotations and lightmaps.</param>
	public delegate void PartRenderer(Part part, Vector2 position, SolutionEditorBase editor, RenderHelper renderer);
}
