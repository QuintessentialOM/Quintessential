using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using MonoMod.Utils;

namespace Quintessential;

using PartType = class_139;
using RenderHelper = class_195;
using PartTypes = class_191;
using AtomTypes = class_175;
using Song = class_186;
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

	#region Sound APIs
	private static Dictionary<string, Sound> SoundBank;
	//for some reason i can't initialize SoundBank the same way i can initialize TextureBank
	//the game crashes before it finishes loading
	//so i initialize this way instead. yuck - mr_puzzel
	public static void initializeSoundDictionary()
	{
		if (SoundBank != null) return;
		SoundBank = new()
		{
			{ "sounds/click_button"         , class_238.field_1991.field_1821 },
			{ "sounds/click_deselect"       , class_238.field_1991.field_1822 },
			{ "sounds/click_select"         , class_238.field_1991.field_1823 },
			{ "sounds/click_story"          , class_238.field_1991.field_1824 },
			{ "sounds/close_enter"          , class_238.field_1991.field_1825 },
			{ "sounds/close_leave"          , class_238.field_1991.field_1826 },
			{ "sounds/code_button"          , class_238.field_1991.field_1827 },
			{ "sounds/code_failure"         , class_238.field_1991.field_1828 },
			{ "sounds/code_success"         , class_238.field_1991.field_1829 },
			{ "sounds/fanfare_solving1"     , class_238.field_1991.field_1830 },
			{ "sounds/fanfare_solving2"     , class_238.field_1991.field_1831 },
			{ "sounds/fanfare_solving3"     , class_238.field_1991.field_1832 },
			{ "sounds/fanfare_solving4"     , class_238.field_1991.field_1833 },
			{ "sounds/fanfare_solving5"     , class_238.field_1991.field_1834 },
			{ "sounds/fanfare_solving6"     , class_238.field_1991.field_1835 },
			{ "sounds/fanfare_story1"       , class_238.field_1991.field_1836 },
			{ "sounds/fanfare_story2"       , class_238.field_1991.field_1837 },
			{ "sounds/glyph_animismus"      , class_238.field_1991.field_1838 },
			{ "sounds/glyph_bonding"        , class_238.field_1991.field_1839 },
			{ "sounds/glyph_calcification"  , class_238.field_1991.field_1840 },
			{ "sounds/glyph_dispersion"     , class_238.field_1991.field_1841 },
			{ "sounds/glyph_disposal"       , class_238.field_1991.field_1842 },
			{ "sounds/glyph_duplication"    , class_238.field_1991.field_1843 },
			{ "sounds/glyph_projection"     , class_238.field_1991.field_1844 },
			{ "sounds/glyph_purification"   , class_238.field_1991.field_1845 },
			{ "sounds/glyph_triplex1"       , class_238.field_1991.field_1846 },
			{ "sounds/glyph_triplex2"       , class_238.field_1991.field_1847 },
			{ "sounds/glyph_triplex3"       , class_238.field_1991.field_1848 },
			{ "sounds/glyph_unbonding"      , class_238.field_1991.field_1849 },
			{ "sounds/glyph_unification"    , class_238.field_1991.field_1850 },
			{ "sounds/instruction_pickup"   , class_238.field_1991.field_1851 },
			{ "sounds/instruction_place"    , class_238.field_1991.field_1852 },
			{ "sounds/instruction_remove"   , class_238.field_1991.field_1853 },
			{ "sounds/piece_modify"         , class_238.field_1991.field_1854 },
			{ "sounds/piece_pickup"         , class_238.field_1991.field_1855 },
			{ "sounds/piece_place"          , class_238.field_1991.field_1856 },
			{ "sounds/piece_remove"         , class_238.field_1991.field_1857 },
			{ "sounds/piece_rotate"         , class_238.field_1991.field_1858 },
			{ "sounds/release_button"       , class_238.field_1991.field_1859 },
			{ "sounds/sim_error"            , class_238.field_1991.field_1860 },
			{ "sounds/sim_start"            , class_238.field_1991.field_1861 },
			{ "sounds/sim_step"             , class_238.field_1991.field_1862 },
			{ "sounds/sim_stop"             , class_238.field_1991.field_1863 },
			{ "sounds/solitaire_end"        , class_238.field_1991.field_1864 },
			{ "sounds/solitaire_match"      , class_238.field_1991.field_1865 },
			{ "sounds/solitaire_select"     , class_238.field_1991.field_1866 },
			{ "sounds/solitaire_start"      , class_238.field_1991.field_1867 },
			{ "sounds/solution"             , class_238.field_1991.field_1868 },
			{ "sounds/title"                , class_238.field_1991.field_1869 },
			{ "sounds/ui_complete"          , class_238.field_1991.field_1870 },
			{ "sounds/ui_fade"              , class_238.field_1991.field_1871 },
			{ "sounds/ui_modal"             , class_238.field_1991.field_1872 },
			{ "sounds/ui_modal_close"       , class_238.field_1991.field_1873 },
			{ "sounds/ui_paper"             , class_238.field_1991.field_1874 },
			{ "sounds/ui_paper_back"        , class_238.field_1991.field_1875 },
			{ "sounds/ui_transition"        , class_238.field_1991.field_1876 },
			{ "sounds/ui_transition_back"   , class_238.field_1991.field_1877 },
			{ "sounds/ui_unlock"            , class_238.field_1991.field_1878 },
		};
	}

	public static void resetSounds()
	{
		foreach (var kvp in SoundBank)
		{
			kvp.Value.field_4062 = false;
		}
	}

	private static void addSoundVolumeEntry(string path, float maxVolume)
	{
		var volumeDictField = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<string, float> volumeDict = (Dictionary<string, float>)volumeDictField.GetValue(null);
		if (!volumeDict.ContainsKey(path))
		{
			volumeDict.Add(path, maxVolume);
			volumeDictField.SetValue(null, volumeDict);
		}
	}

	/// <summary>
	/// Loads a .wav file from disk. Returns the new sound.
	/// </summary>
	/// <param name="path">The file path to the sound.</param>
	/// <param name="maxVolume">The maximum volume of the sound, between 0.0f and 1.0f.</param>
	public static Sound loadSound(string path, float maxVolume = 1f)
	{
		//load sound
		string maxVolumeStr = maxVolume.ToString();
		var sound = class_235.method_616(path);
		addSoundVolumeEntry(sound.field_4060, maxVolume);
		SoundBank.Add(path, sound);
		return sound;
	}

	/// <summary>
	/// Returns an already-loaded sound associated with the file path.
	/// </summary>
	/// <param name="path">The file path to the sound.</param>
	public static Sound fetchSound(string path)
	{
		if (SoundBank.ContainsKey(path))
		{
			return SoundBank[path];
		}
		else
		{
			throw new ThrowError($"QApi.fetchSound: can't find \"{path}\" - did you forget to load it?");
		}
	}

	/// <summary>
	/// Plays a sound with the specified volume.
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="volume">Desired volume, between 0.0f (muted) and 1.0f (full volume).</param>
	public static void playSound(Sound sound, float volume = 1f)
	{
		class_11.method_28(sound, volume);
	}

	/// <summary>
	/// Plays a sound with the max volume multiplied by the result of QAPI.getVolumeFactor(sim).
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="sim">The current Sim.</param>
	public static void playSound(Sound sound, Sim sim = null)
	{
		class_11.method_28(sound, getVolumeFactor(sim));
	}

	/// <summary>
	/// Plays a sound with the max volume multiplied by the result of QAPI.getVolumeFactor(seb).
	/// </summary>
	/// <param name="sound">The sound to play.</param>
	/// <param name="seb">The current SolutionEditorBase.</param>
	public static void playSound(Sound sound, SolutionEditorBase seb = null)
	{
		class_11.method_28(sound, getVolumeFactor(null, seb));
	}

	/// <summary>
	/// Returns a volume factor depending on the gameplay situation:
	/// - If we are recording a GIF, return 0.0f.
	/// - If we are simulating a solution in Quick mode, return 0.5f.
	/// - Otherwise, return 1.0f.
	/// </summary>
	/// <param name="sim">The current Sim.</param>
	public static float getVolumeFactor(Sim sim)
	{
		return getVolumeFactor(sim, null);
	}

	/// <summary>
	/// Returns a volume factor depending on the gameplay situation:
	/// - If we are recording a GIF, return 0.0f.
	/// - If we are simulating a solution in Quick mode, return 0.5f.
	/// - Otherwise, return 1.0f.
	/// </summary>
	/// <param name="seb">The current SolutionEditorBase.</param>
	public static float getVolumeFactor(SolutionEditorBase seb)
	{
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

	#region Song APIs
	private static Dictionary<string, Song> SongBank = new()
	{
		{"music/Map"         ,class_238.field_1992.field_968},
		{"music/Solitaire"   ,class_238.field_1992.field_969},
		{"music/Solving1"    ,class_238.field_1992.field_970},
		{"music/Solving2"    ,class_238.field_1992.field_971},
		{"music/Solving3"    ,class_238.field_1992.field_972},
		{"music/Solving4"    ,class_238.field_1992.field_973},
		{"music/Solving5"    ,class_238.field_1992.field_974},
		{"music/Solving6"    ,class_238.field_1992.field_975},
		{"music/Story1"      ,class_238.field_1992.field_976},
		{"music/Story2"      ,class_238.field_1992.field_977},
		{"music/Title"       ,class_238.field_1992.field_978},
	};

	/// <summary>
	/// Loads a .ogg file from disk. Returns the new song.
	/// </summary>
	/// <param name="path">The file path to the song.</param>
	public static Song loadSong(string path)
	{
		var song = class_235.method_617(path);
		SongBank.Add(path, song);
		return song;
	}

	/// <summary>
	/// Returns the already-loaded song associated with the file path.
	/// </summary>
	/// <param name="path">The file path to the song.</param>
	public static Song fetchSong(string path)
	{
		if (SongBank.ContainsKey(path))
		{
			return SongBank[path];
		}
		else
		{
			throw new ThrowError($"QApi.fetchSong: can't find \"{path}\" - did you forget to load it?");
		}
	}
	#endregion

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
