using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintessential {

	using PartType = class_139;
	using RenderHelper = class_195;
	using PartTypes = class_191;
	using AtomTypes = class_175;

	public static class QApi {

		public static readonly List<Pair<Predicate<Part>, PartRenderer>> PartRenderers = new List<Pair<Predicate<Part>, PartRenderer>>();
		public static readonly List<Pair<PartType, PartType>> PanelParts = new List<Pair<PartType, PartType>>();
		public static readonly List<AtomType> ModAtomTypes = new List<AtomType>();
		public static readonly List<Action<Sim, bool>> ToRunAfterCycle = new List<Action<Sim, bool>>();

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

		/// <summary>
		/// Adds an atom type, adding it to the list of atom types and the molecule editor.
		/// </summary>
		/// <param name="type">The atom type to add.</param>
		public static void AddAtomType(AtomType type) {
			ModAtomTypes.Add(type);

			Array.Resize(ref AtomTypes.field_1691, AtomTypes.field_1691.Length + 1);
			var len = AtomTypes.field_1691.Length;
			AtomTypes.field_1691[len - 1] = type;
		}

		/// <summary>
		/// Runs the given action at the end of every half-cycle.
		/// </summary>
		/// <param name="runnable">An action to be run every half-cycle, given the sim and whether it is the start or end.</param>
		public static void RunAfterCycle(Action<Sim, bool> runnable) {
			ToRunAfterCycle.Add(runnable);
		}
	}

	/// <summary>
	/// A function that renders a part.
	/// </summary>
	/// <param name="part">The part to be displayed.</param>
	/// <param name="position">The position of the part.</param>
	/// <param name="editor">The solution editor that the part is being displayed in.</param>
	/// <param name="helper">An object containing functions for rendering images, at different positions/rotations and lightmaps.</param>
	public delegate void PartRenderer(Part part, Vector2 position, SolutionEditorBase editor, RenderHelper helper);

	/// <summary>
	/// A static class containing extensions that make PartRenderers easier to use.
	/// </summary>
	public static class PartRendererExtensions {

		public static PartRenderer Then(this PartRenderer first, PartRenderer second) {
			return (a, b, c, d) => {
				first(a, b, c, d);
				second(a, b, c, d);
			};
		}

		public static PartRenderer WithOffsets(this PartRenderer renderer, params Vector2[] offsets) {
			return (part, pos, editor, helper) => {
				foreach(var offset in offsets)
					renderer(part, pos + offset, editor, helper);
			};
		}

		/*public static PartRenderer WithOffsets(this PartRenderer renderer, params HexIndex[] offsets) {
			const double angle = (1/3) * Math.PI;
			return renderer.WithOffsets(offsets.Select(off => new Vector2((float)(off.Q + Math.Cos(angle) * off.R), -(float)(Math.Sin(angle) * off.R))).ToArray());
		}*/

		public static PartRenderer OfTexture(class_256 texture, params HexIndex[] hexes) {
			return (part, pos, editor, helper) => {
				foreach(var hex in hexes)
					helper.method_528(texture, hex, Vector2.Zero);
			};
		}

		public static PartRenderer OfTexture(string texture, params HexIndex[] hexes) {
			return OfTexture(class_235.method_615(texture), hexes);
		}
	}
}
