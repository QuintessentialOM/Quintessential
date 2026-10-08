using System;
using System.Collections.Generic;

namespace Quintessential;

/// <summary>
/// Api methods for registering <see cref="AtomTypes"/>, <see cref="PartTypes"/> and many more.
/// </summary>
public static class QApi {

	public static readonly List<Tuple<Predicate<Part>, PartRendererDelegate>> PartRenderers = [];
	public static readonly List<Tuple<PartType, PartType>> PanelParts = [];
	public static readonly List<AtomType> ModAtomTypes = [];
	public static readonly List<Action<Sim, bool>> ToRunAfterCycle = [];
	public static readonly List<Tuple<string, SolutionPayloadHandler>> SolutionPayloadHandler = [];
	public static readonly List<PuzzleOption> PuzzleOptions = [];

	/// <summary>
	/// Adds a part type to the end of a part panel section, making it accessible for placement.<br/>
	/// This does not allow for adding inputs or outputs.
	/// </summary>
	/// <param name="type">The part type to be added.</param>
	/// <param name="mechanism">Whether to add to the mechanisms section or the glyphs section.</param>
	public static void AddPartTypeToPanel(PartType type, bool mechanism){
		AddPartTypeToPanel(type, mechanism ? PartTypes.berlosWheel : PartTypes.equilibriumGlyph);
	}

	/// <summary>
	/// Adds a part type to the part panel after another given type, making it accessible for placement.
	/// </summary>
	/// <param name="type">The part type to be added.</param>
	/// <param name="after">The part type after which the other one gets added.</param>
	public static void AddPartTypeToPanel(PartType type, PartType after) {
		if(type == null || after == null)
			Logger.Log("Tried to add a null part to the parts panel, or tried to add a part after a null part, not adding.");
		else if(type.Equals(after))
			Logger.Log("Tried to add a part to the part panel after itself (circular reference), not adding.");
		else
			PanelParts.Add(new Tuple<PartType, PartType>(type, after));
	}

	// TODO check the part type before calling the delegate for performance reasons.
	/// <summary>
	/// Adds a PartRenderer, which renders any parts that satisfy the given predicate. Usually, this predicate simply checks the part type of the part.
	/// </summary>
	/// <param name="mod">The mod that adds the renderer.</param>
	/// <param name="renderer">The PartRenderer to be added and displayed.</param>
	/// <param name="typeChecker">A predicate that determines which parts the renderer should try to display.</param>
	public static void AddPartTypesRenderer(this QuintessentialMod mod, PartRendererDelegate renderer, Predicate<Part> typeChecker) {
		PartRenderers.Add(new Tuple<Predicate<Part>, PartRendererDelegate>(typeChecker, renderer));
	}

	/// <summary>
	/// Adds a part type to the list of all part types.
	/// </summary>
	/// <param name="mod">The mod that adds the part.</param>
	/// <param name="type">The part type to be added.</param>
	/// <param name="id">The <b>name</b> of the id of the part.</param>
	public static void AddPartType(this QuintessentialMod mod, PartType type, string id) {
	((patch_PartType)(object)type).Id = mod.GetIdentifier(id);
		type.id = mod.GetIdentifier(id);
		type.name = mod.Translate("parts." + id);
		type.description = mod.Translate("parts." + id + ".description");

		Array.Resize(ref PartTypes.partTypes, PartTypes.partTypes.Length + 1);
		PartTypes.partTypes[^1] = type;
	}

	/// <summary>
	/// Adds a part type, adding it to the list of part types and adding a renderer for that part type.
	/// </summary>
	/// <param name="mod">The mod that adds the part.</param>
	/// <param name="type">The part type to be added.</param>
	/// <param name="id">The <b>name</b> of the id of the part.</param>
	/// <param name="renderer">A PartRenderer to render instances of that part type.</param>
	public static void AddPartType(this QuintessentialMod mod, PartType type, string id, PartRendererDelegate renderer) {
		mod.AddPartType(type, id);
		mod.AddPartTypesRenderer(renderer, part => part.GetType() == type);
	}

	/// <summary>
	/// Adds an atom type, adding it to the list of atom types and the molecule editor.
	/// </summary>
	/// <param name="mod">The mod that adds the atom.</param>
	/// <param name="type">The atom type to add.</param>
	/// <param name="id">The <b>name</b> of the id of the atom.</param>
	public static void AddAtomType(this QuintessentialMod mod, AtomType type, string id) {
		type.byteId = 255; // doesn't really matter - should not overlap vanilla atom ids
		((patch_AtomType)(object)type).Id = mod.GetIdentifier(id);
		type.name = mod.Translate("atoms." + id);
		type.elementalName = mod.Translate("atoms." + id + ".elemental");
		type.defaultName = mod.Translate("atoms." + id).locDictionary[Language.English];

		ModAtomTypes.Add(type);
		Array.Resize(ref AtomTypes.atoms, AtomTypes.atoms.Length + 1);
		var len = AtomTypes.atoms.Length;
		AtomTypes.atoms[len - 1] = type;
	}

	/// <summary>
	/// Runs the given action for every part on each half-cycle.
	/// </summary>
	/// <param name="runnable">An action to be run for every part, given the sim, part, partSimState, and whether it is the start or end.</param>
	[Obsolete("Use BindGlyphCycle instead from the Data Module of Quintessential.")]
	public static void RunDuringCycle(Action<Sim, Part, PartSimState, bool> runnable) { }

	/// <summary>
	/// Runs the given action at the end of every half-cycle.
	/// </summary>
	/// <param name="runnable">An action to be run every half-cycle, given the sim and whether it is the start or end.</param>
	public static void RunAfterCycle(Action<Sim, bool> runnable) {
		ToRunAfterCycle.Add(runnable);
	}

	/// <summary>
	/// Adds a permission to the puzzle editor. These can be used by setting the `CustomPermissionCheck` field of your part type and
	/// checking for your permission ID.
	/// <para>
	/// Permissions with the same section name will be grouped together. If no name is chosen, this defaults to "Other Parts &amp; Mechanisms".
	/// </para>
	/// </summary>
	/// <param name="mod">The mod that adds the permission.</param>
	/// <param name="id">The <b>name</b> of the id of the permission.</param>
	/// <param name="displayName">An override for the key of the permission display name, the defalt is <paramref name="id"/>.</param>
	/// <param name="sectionName">
	/// An override for the key of the section display name that the permission will appear under.<br/>
	/// The default is <b><i>permission_sections</i></b>
	/// </param>
	/// <param name="length"></param> // TODO What on Earth is this used for? Figure that one out already... please.
	public static void AddPuzzlePermission(this QuintessentialMod mod, string id, string displayName = "", string sectionName = "", int length = 2) {
		if (displayName == "") displayName = id;
		var sectionNameLoc = mod.Translate(
			"permission_sections" + (sectionName == "" ? "" : "." + sectionName)
		);
		var displayNameLoc = mod.Translate("permissions." + displayName );

		PuzzleOptions.Add(PuzzleOption.BoolOption(mod.GetIdentifier(id), displayNameLoc, sectionNameLoc, length));
	}

	/// <summary>
	/// Register a <see cref="PuzzleOption"/>.
	/// </summary>
	/// <param name="option">The <see cref="PuzzleOption"/> to register.</param>
	public static void AddPuzzleOption(PuzzleOption option){
		PuzzleOptions.Add(option);
	}

	// TODO improve on this...
	/// <summary>
	/// 
	/// </summary>
	/// <param name="address"></param>
	/// <param name="handler"></param>
	public static void AddSolutionPayloadHandler(string address, SolutionPayloadHandler handler)
	{
		SolutionPayloadHandler.Add(new(address, handler));
	}



	/// <summary>
	/// Adds a chamber type, used by name in production puzzle files.
	/// </summary>
	/// <param name="mod">The mod that adds the chamber.</param>
	/// <param name="id">The <b>name</b> of the id of the chamber.</param>
	/// <param name="productionChamber">The chamber type to add.</param>
	/// <param name="autoCentre">
	/// Whether to automatically assign a centred offset for the chamber's overlay texture.<br/>
	/// Otherwise, the chamber's <c>field_1730</c> must have its offset<br/>
	/// assigned by <c>UI.AssignOffset</c>, or the chamber will be visually incorrect.
	/// </param>
	public static void AddProductionChamber(this QuintessentialMod mod, string id, ProductionChamber productionChamber, bool autoCentre = true){
		productionChamber.name = mod.GetIdentifier(id);

		int length = Puzzles.prodChambers.Length;
		Array.Resize(ref Puzzles.prodChambers, length + 1);
		Puzzles.prodChambers[length] = productionChamber;

		if(autoCentre)
			UI.AssignOffset(productionChamber.borderTexture, -0.5f * productionChamber.borderTexture.size.ToVector2());
	}

	/// <summary>
	/// Returns the settings of the given type for the first registered mod, or null if no registered mod has settings of that type.
	/// </summary>
	/// <typeparam name="T">The type of settings to get.</typeparam>
	/// <returns></returns>
	public static T GetSettingsByType<T>() {
		foreach(var mod in QuintessentialLoader.CodeMods) {
			if(mod.Settings is T settings) {
				return settings;
			}
		}
		return default;
	}
}

/// <summary>
/// A function that renders a part.
/// </summary>
/// <param name="part">The part to be displayed.</param>
/// <param name="position">The position of the part.</param>
/// <param name="editor">The solution editor that the part is being displayed in.</param>
/// <param name="helper">An object containing functions for rendering images, at different positions/rotations and lightmaps.</param>
public delegate void PartRendererDelegate(Part part, Vector2 position, SolutionEditorBase editor, PartRenderer helper);

public delegate void SolutionPayloadHandler(Solution solution, string data);

/// <summary>
/// A static class containing extensions that make PartRenderers easier to use.
/// </summary>
public static class PartRendererExtensions {

	/// <summary>
	/// Combine the two delegates.
	/// </summary>
	/// <param name="first">The first one to run.</param>
	/// <param name="second">The second one to run.</param>
	/// <returns>The combined delegate.</returns>
	public static PartRendererDelegate Then(this PartRendererDelegate first, PartRendererDelegate second) {
		return (a, b, c, d) => {
			first(a, b, c, d);
			second(a, b, c, d);
		};
	}

	/// <summary>
	/// Copy the delegate for each offset.
	/// </summary>
	/// <param name="renderer">The delegate to use.</param>
	/// <param name="offsets">The offsets to use.</param>
	/// <returns>The delegate doing all of the rendering.</returns>
	public static PartRendererDelegate WithOffsets(this PartRendererDelegate renderer, params Vector2[] offsets) {
		return (part, pos, editor, helper) => {
			foreach(var offset in offsets)
				renderer(part, pos + offset, editor, helper);
		};
	}

	/*public static PartRenderer WithOffsets(this PartRenderer renderer, params HexIndex[] offsets) {
		const double angle = (1/3) * Math.PI;
		return renderer.WithOffsets(offsets.Select(off => new Vector2((float)(off.Q + Math.Cos(angle) * off.R), -(float)(Math.Sin(angle) * off.R))).ToArray());
	}*/

	/// <summary>
	/// Render the <see cref="Texture"/> at each <see cref="HexIndex"/>.
	/// </summary>
	/// <param name="texture">The <see cref="Texture"/> to render.</param>
	/// <param name="hexes"><see cref="HexIndex"/>-s to render the textures at.</param>
	/// <returns>The delegate doing all of the rendering.</returns>
	public static PartRendererDelegate OfTexture(Texture texture, params HexIndex[] hexes) {
		return (part, pos, editor, helper) => {
			foreach(var hex in hexes)
				helper.RenderRotating(texture, hex, Vector2.Zero);
		};
	}

	/// <summary>
	/// Render the <see cref="Texture"/> at each <see cref="HexIndex"/>.
	/// </summary>
	/// <param name="texture">A path to the <see cref="Texture"/> to render.</param>
	/// <param name="hexes"><see cref="HexIndex"/>-s to render the textures at.</param>
	/// <returns>The delegate doing all of the rendering.</returns>
	public static PartRendererDelegate OfTexture(string texture, params HexIndex[] hexes) {
		return OfTexture(AssetLoaderHelper.LoadTexture(texture), hexes);
	}
}
