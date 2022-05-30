using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using MonoMod.Utils;

namespace Quintessential;

using PartType = class_139;
using RenderHelper = class_195;
using PartTypes = class_191;
using AtomTypes = class_175;
using ThrowError = class_266;

public static class QApi
{
	public static readonly List<Pair<Predicate<Part>, PartRenderer>> PartRenderers = new();
	public static readonly List<Pair<PartType, PartType>> PanelParts = new();
	public static readonly List<AtomType> ModAtomTypes = new();
	public static readonly List<Action<Sim, bool>> ToRunAfterCycle = new();

	public static void Init()
	{
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
		filePath = Path.Combine(filePath, fileType);
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
		throw new ThrowError("QApi.fetchPath: the file ' " + filePath + " ' does not exist in any mod's content directory!");
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



	#region Sound APIs
	private static readonly List<Sound> AllSounds = new();
	private static readonly Dictionary<OMSound, Sound> OMSounds = new()
	{
		{ OMSound.click_button, class_238.field_1991.field_1821 },
		{ OMSound.click_deselect, class_238.field_1991.field_1822 },
		{ OMSound.click_select, class_238.field_1991.field_1823 },
		{ OMSound.click_story, class_238.field_1991.field_1824 },
		{ OMSound.close_enter, class_238.field_1991.field_1825 },
		{ OMSound.close_leave, class_238.field_1991.field_1826 },
		{ OMSound.code_button, class_238.field_1991.field_1827 },
		{ OMSound.code_failure, class_238.field_1991.field_1828 },
		{ OMSound.code_success, class_238.field_1991.field_1829 },
		{ OMSound.fanfare_solving1, class_238.field_1991.field_1830 },
		{ OMSound.fanfare_solving2, class_238.field_1991.field_1831 },
		{ OMSound.fanfare_solving3, class_238.field_1991.field_1832 },
		{ OMSound.fanfare_solving4, class_238.field_1991.field_1833 },
		{ OMSound.fanfare_solving5, class_238.field_1991.field_1834 },
		{ OMSound.fanfare_solving6, class_238.field_1991.field_1835 },
		{ OMSound.fanfare_story1, class_238.field_1991.field_1836 },
		{ OMSound.fanfare_story2, class_238.field_1991.field_1837 },
		{ OMSound.glyph_animismus, class_238.field_1991.field_1838 },
		{ OMSound.glyph_bonding, class_238.field_1991.field_1839 },
		{ OMSound.glyph_calcification, class_238.field_1991.field_1840 },
		{ OMSound.glyph_dispersion, class_238.field_1991.field_1841 },
		{ OMSound.glyph_disposal, class_238.field_1991.field_1842 },
		{ OMSound.glyph_duplication, class_238.field_1991.field_1843 },
		{ OMSound.glyph_projection, class_238.field_1991.field_1844 },
		{ OMSound.glyph_purification, class_238.field_1991.field_1845 },
		{ OMSound.glyph_triplex1, class_238.field_1991.field_1846 },
		{ OMSound.glyph_triplex2, class_238.field_1991.field_1847 },
		{ OMSound.glyph_triplex3, class_238.field_1991.field_1848 },
		{ OMSound.glyph_unbonding, class_238.field_1991.field_1849 },
		{ OMSound.glyph_unification, class_238.field_1991.field_1850 },
		{ OMSound.instruction_pickup, class_238.field_1991.field_1851 },
		{ OMSound.instruction_place, class_238.field_1991.field_1852 },
		{ OMSound.instruction_remove, class_238.field_1991.field_1853 },
		{ OMSound.piece_modify, class_238.field_1991.field_1854 },
		{ OMSound.piece_pickup, class_238.field_1991.field_1855 },
		{ OMSound.piece_place, class_238.field_1991.field_1856 },
		{ OMSound.piece_remove, class_238.field_1991.field_1857 },
		{ OMSound.piece_rotate, class_238.field_1991.field_1858 },
		{ OMSound.release_button, class_238.field_1991.field_1859 },
		{ OMSound.sim_error, class_238.field_1991.field_1860 },
		{ OMSound.sim_start, class_238.field_1991.field_1861 },
		{ OMSound.sim_step, class_238.field_1991.field_1862 },
		{ OMSound.sim_stop, class_238.field_1991.field_1863 },
		{ OMSound.solitaire_end, class_238.field_1991.field_1864 },
		{ OMSound.solitaire_match, class_238.field_1991.field_1865 },
		{ OMSound.solitaire_select, class_238.field_1991.field_1866 },
		{ OMSound.solitaire_start, class_238.field_1991.field_1867 },
		{ OMSound.solution, class_238.field_1991.field_1868 },
		{ OMSound.title, class_238.field_1991.field_1869 },
		{ OMSound.ui_complete, class_238.field_1991.field_1870 },
		{ OMSound.ui_fade, class_238.field_1991.field_1871 },
		{ OMSound.ui_modal, class_238.field_1991.field_1872 },
		{ OMSound.ui_modal_close, class_238.field_1991.field_1873 },
		{ OMSound.ui_paper, class_238.field_1991.field_1874 },
		{ OMSound.ui_paper_back, class_238.field_1991.field_1875 },
		{ OMSound.ui_transition, class_238.field_1991.field_1876 },
		{ OMSound.ui_transition_back, class_238.field_1991.field_1877 },
		{ OMSound.ui_unlock, class_238.field_1991.field_1878 },
	};

	public static void resetSounds() { foreach (var sound in AllSounds) sound.field_4062 = false; }

	/// <summary>
	/// Loads a .wav file from disk. Returns the new Sound.
	/// </summary>
	/// <param name="path">The file path to the sound.</param>
	/// <param name="maxVolume">The maximum volume of the sound, between 0.0f and 1.0f inclusive.</param>
	public static Sound loadSound(string path, float maxVolume = 1f)
	{
		//load sound
		string maxVolumeStr = maxVolume.ToString();
		var sound = class_235.method_616(path);
		sound.field_4060 = maxVolumeStr;

		//add volume entry to class_11.field_52
		var volumeDictField = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<string, float> volumeDict = (Dictionary<string, float>)volumeDictField.GetValue(null);
		if (!volumeDict.ContainsKey(maxVolumeStr))
		{
			volumeDict.Add(maxVolumeStr, maxVolume);
			volumeDictField.SetValue(null, volumeDict);
		}
		//add sound to AllSounds, so method_504 can reset field_4062 when required
		AllSounds.Add(sound);

		return sound;
	}

	/// <summary>
	/// Returns the vanilla Sound associated with the OMSoundID.
	/// </summary>
	/// <param name="id">The ID of the sound.</param>
	public static Sound fetchSound(OMSound id) {
		return OMSounds[id];
	}

	/// <summary>
	/// Plays a Sound with the specified volume.
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="volume">Desired volume, between 0.0f (muted) and 1.0f (full volume).</param>
	public static void playSound(Sound sound, float volume = 1f) {
		class_11.method_28(sound, volume);
	}

	/// <summary>
	/// Plays a Sound with the max volume multiplied by the result of QAPI.getVolumeFactor(sim).
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="sim">The current Sim.</param>
	public static void playSound(Sound sound, Sim sim = null) {
		class_11.method_28(sound, getVolumeFactor(sim));
	}

	/// <summary>
	/// Plays a Sound with the max volume multiplied by the result of QAPI.getVolumeFactor(seb).
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="seb">The current SolutionEditorBase.</param>
	public static void playSound(Sound sound, SolutionEditorBase seb = null) {
		class_11.method_28(sound, getVolumeFactor(seb));
	}

	/// <summary>
	/// Returns a volume factor depending on the gameplay situation:
	/// - If we are recording a GIF, return 0.0f.
	/// - If we are simulating a solution in Quick mode, return 0.5f.
	/// - Otherwise, return 1.0f.
	/// </summary>
	/// <param name="sim">The current Sim.</param>
	public static float getVolumeFactor(Sim sim) {
		return getVolumeFactor(sim, null);
	}

	/// <summary>
	/// Returns a volume factor depending on the gameplay situation:
	/// - If we are recording a GIF, return 0.0f.
	/// - If we are simulating a solution in Quick mode, return 0.5f.
	/// - Otherwise, return 1.0f.
	/// </summary>
	/// <param name="seb">The current SolutionEditorBase.</param>
	public static float getVolumeFactor(SolutionEditorBase seb) {
		return getVolumeFactor(null, seb);
	}

	private static float getVolumeFactor(Sim sim = null, SolutionEditorBase seb = null)
	{
		if (sim != null)
		{
			seb = new DynamicData(sim).Get<SolutionEditorBase>("field_3818");
		}
		if (seb != null)
		{
			if (seb is class_194) // GIF recording, so mute
			{
				return 0.0f;
			}
			else if (seb is SolutionEditorScreen)
			{
				var seb_dyn = new DynamicData(seb);
				bool isQuickMode = seb_dyn.Get<Maybe<int>>("field_4030").method_1085();
				return isQuickMode ? 0.5f : 1f;
			}
		}
		return 1f;
	}
	#endregion
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

public enum OMSound : int
{
	click_button,
	click_deselect,
	click_select,
	click_story,
	close_enter,
	close_leave,
	code_button,
	code_failure,
	code_success,
	fanfare_solving1,
	fanfare_solving2,
	fanfare_solving3,
	fanfare_solving4,
	fanfare_solving5,
	fanfare_solving6,
	fanfare_story1,
	fanfare_story2,
	glyph_animismus,
	glyph_bonding,
	glyph_calcification,
	glyph_dispersion,
	glyph_disposal,
	glyph_duplication,
	glyph_projection,
	glyph_purification,
	glyph_triplex1,
	glyph_triplex2,
	glyph_triplex3,
	glyph_unbonding,
	glyph_unification,
	instruction_pickup,
	instruction_place,
	instruction_remove,
	piece_modify,
	piece_pickup,
	piece_place,
	piece_remove,
	piece_rotate,
	release_button,
	sim_error,
	sim_start,
	sim_step,
	sim_stop,
	solitaire_end,
	solitaire_match,
	solitaire_select,
	solitaire_start,
	solution,
	title,
	ui_complete,
	ui_fade,
	ui_modal,
	ui_modal_close,
	ui_paper,
	ui_paper_back,
	ui_transition,
	ui_transition_back,
	ui_unlock,
}