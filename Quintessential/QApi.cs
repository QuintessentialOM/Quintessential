using System;
using System.IO;
using System.Collections.Generic;

namespace Quintessential;

using PartType = class_139;
using RenderHelper = class_195;
using PartTypes = class_191;
using AtomTypes = class_175;
using Texture = class_256;
using ThrowError = class_266;

public static class QApi {

	public static readonly List<Pair<Predicate<Part>, PartRenderer>> PartRenderers = new();
	public static readonly List<Pair<PartType, PartType>> PanelParts = new();
	public static readonly List<AtomType> ModAtomTypes = new();
	public static readonly List<Action<Sim, bool>> ToRunAfterCycle = new();
	// ID, display name
	public static readonly List<Pair<string, string>> CustomPermisions = new();

	public static void Init() {
		//
	}

	/// <summary>
	/// Returns the filepath to the mod directory containing the specified file.
	/// If fileType is not specified, then it is assumed that filePath includes the file extension.
	/// </summary>
	/// <param name="filePath">The filepath for the file, where 'Content' is treated as the root directory.</param>
	/// <param name="fileType">The extension (e.g. ".wav") for the file.</param>
	public static string fetchPath(string filePath, string fileType = "")
	{
		filePath = filePath + fileType;
		string path = Path.Combine("Content", filePath);
		for (int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--)
		{
			string dir = Path.Combine(QuintessentialLoader.ModContentDirectories[i], "Content");
			path = Path.Combine(dir, filePath);
			if (File.Exists(path))
			{
				return path;
			}
		}
		path = Path.Combine("Content", filePath);
		if (File.Exists(path))
		{
			return path;
		}
		throw new ThrowError($"QApi.fetchPath: the file \"{filePath}\" does not exist in any mod's content directory!");
	}

	#region Texture APIs
	private static Dictionary<string, Texture> TextureBank = new()
	{
		//not all textures - mainly backgrounds, letters, and icons useful for custom campaigns
		{"textures/cinematic/backgrounds/greathall_a",  class_238.field_1989.field_84.field_535.field_536},
		{"textures/cinematic/backgrounds/greathall_b",  class_238.field_1989.field_84.field_535.field_537},
		{"textures/cinematic/backgrounds/greathall_c",  class_238.field_1989.field_84.field_535.field_538},
		{"textures/cinematic/backgrounds/tailor_a",     class_238.field_1989.field_84.field_535.field_539},
		{"textures/cinematic/backgrounds/tailor_b",     class_238.field_1989.field_84.field_535.field_540},
		{"textures/cinematic/backgrounds/tailor_c",     class_238.field_1989.field_84.field_535.field_541},
		{"textures/cinematic/backgrounds/workshop",     class_238.field_1989.field_84.field_535.field_542},
	};

	/// <summary>
	/// Loads a .png or .psd texture from disk. Returns the new Texture.
	/// </summary>
	/// <param name="path">The file path to the texture.</param>
	/// <param name="store">If true, stores the texture in the Texture Bank, which can be accessed using fetchTexture.</param>
	public static Texture loadTexture(string path, bool store = false)
	{
		var texture = class_235.method_615(path);
		if (store)
		{
			TextureBank.Add(path, texture);
		}
		return texture;
	}

	/// <summary>
	/// Returns the texture associated with the file path from the Texture Bank.
	/// </summary>
	/// <param name="path">The file path to the texture.</param>
	public static Texture fetchTexture(string path)
	{
		if (TextureBank.ContainsKey(path))
		{
			return TextureBank[path];
		}
		else
		{
			throw new ThrowError($"QApi.fetchTexture: can't find \"{path}\" in the Texture Bank - did you forget to store it?");
		}
	}
	#endregion

	#region Part APIs
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
	#endregion

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

	/// <summary>
	/// Adds a permission to the "More Options" section of the puzzle editor. These can be used by setting the `CustomPermissionCheck`
	/// field of your part type and checking for your permission ID.
	/// </summary>
	/// <param name="id">The ID of the permission that is used during checks and saved to puzzle files.</param>
	/// <param name="displayName">The name of the permission that is displayed in the UI, e.g. "Glyphs of Quintessence".</param>
	public static void AddPuzzlePermission(string id, string displayName) {
		CustomPermisions.Add(new(id, displayName));
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
