using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Texture = class_256;
using Font = class_1;

using Ionic.Zip;

using MonoMod.Utils;

using Quintessential.Serialization;

namespace Quintessential;

public class QuintessentialLoader {

	public readonly static string VersionString = "0.3.0";
	public readonly static int VersionNumber = 6;

	public static string PathLightning;
	public static string PathMods;
	public static string PathBlacklist;
	public static string PathModSaves;

	public static List<QuintessentialMod> CodeMods = new();
	public static List<ModMeta> Mods = new();
	public static List<string> ModContentDirectories = new();
	public static List<string> ModPuzzleDirectories = new();

	public static ModMeta QuintessentialModMeta;
	public static QuintessentialMod QuintessentialAsMod;

	private static List<string> blacklisted = new();
	private static List<ModMeta> loaded = new();
	private static List<ModMeta> waiting = new();

	private static readonly string zipExtractSuffix = "__quintessential_from_zip";
	private static readonly string quintAssetFolder = "__quintessential_assets";

	#region main methods
	public static void PreInit() {
		try {
			PathLightning = Path.GetDirectoryName(typeof(GameLogic).Assembly.Location);
			PathMods = Path.Combine(PathLightning, "Mods");

			Logger.Init();
			Logger.Log($"Quintessential v{VersionString} ({VersionNumber})");
			Logger.Log("Starting pre-init loading.");

			QApi.Init();

			if(!Directory.Exists(PathMods))
				Directory.CreateDirectory(PathMods);

			PathBlacklist = Path.Combine(PathMods, "blacklist.txt");
			if(File.Exists(PathBlacklist))
				blacklisted = File.ReadAllLines(PathBlacklist).Select(l => (l.StartsWith("#") ? "" : l).Trim()).ToList();
			else {
				File.WriteAllText(PathBlacklist, @"# This is the blacklist. Lines starting with # are ignored.
ExampleFolderThatIWantToBlacklist
SomeZipIDontLike.zip");
			}

			// Find mods in Mods/
			// Delete leftover quintessential extracted zips and assets
			CleanupExtractedData();

			// Add Quintessential mod & mod meta
			QuintessentialModMeta = new ModMeta {
				Name = "Quintessential",
				Version = new Version(VersionString)
			};
			Mods.Add(QuintessentialModMeta);
			QuintessentialAsMod = new Internal.QuintessentialAsMod {
				Meta = QuintessentialModMeta,
				Settings = new QuintessentialSettings()
			};
			CodeMods.Add(QuintessentialAsMod);

			// Extract bundled assets
			Logger.Log("Extracting Quintessential resources...");
			string outDir = Path.Combine(PathMods, quintAssetFolder, "Content", "Quintessential");
			Directory.CreateDirectory(outDir);
			ResourceManager manager = new("Properties.Resources", typeof(Renderer).Assembly);
			var set = manager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
			foreach(object item in set){
				if(item is DictionaryEntry de){
					string name = (string)de.Key;
					using var toStream = File.OpenWrite(Path.Combine(outDir, name));
					byte[] content = (byte[])de.Value;
					toStream.Write(content, 0, content.Length);
				}
			}
			ModContentDirectories.Add(Path.Combine(PathMods, quintAssetFolder));

			Logger.Log("Finding mods to load...");
			// Unzip zips
			string[] files = Directory.GetFiles(PathMods);
			foreach(var file in files) {
				string filename = Path.GetFileName(file);
				if(blacklisted.Contains(filename))
					continue;
				if(filename.EndsWith(".zip"))
					FindZipMod(file);
			}

			// Find folder mods
			string[] folders = Directory.GetDirectories(PathMods);
			foreach(var folder in folders) {
				string filename = Path.GetFileName(folder);
				if(blacklisted.Contains(filename))
					continue;
				FindFolderMod(folder);
			}

			// Load mods
			Logger.Log("Loading mods...");
			bool Contains(ModMeta.Dependency dep, List<ModMeta> list)
				=> list.Any(m => m.Name.Equals(dep.Name) && m.Version >= dep.Version);
			List<ModMeta> rem = new();
			foreach(var mod in Mods) {
				// check dependencies
				bool willLoad = true, wait = false;
				foreach(var dep in mod.Dependencies) {
					// if a dependency is missing, don't load the mod
					if(!Contains(dep, Mods))
						willLoad = false;
					// if a dependency is present but not loaded, add to waiting list and load later
					else if(!Contains(dep, loaded)) {
						willLoad = false;
						wait = true;
					}
				}
				// check optional deps
				foreach(var opDep in mod.OptionalDependencies) {
					// if an optional dep is present but outdated, don't load
					if(Mods.Any(m => m.Name.Equals(opDep.Name) && m.Version < opDep.Version))
						willLoad = false;
					// if an optional dep is present but not yet loaded, add to waiting list and load later
					if(Contains(opDep, Mods) && !Contains(opDep, loaded)) {
						willLoad = false;
						wait = true;
					}
				}
				if(willLoad)
					LoadModFromMeta(mod);
				else if(wait)
					waiting.Add(mod);
				else {
					Logger.Log("Not loading " + mod.Name + ": missing dependency, or outdated optional dependency.");
					rem.Add(mod);
				}
			}
			Mods.RemoveAll(m => rem.Contains(m));

			while(waiting.Count() > 0) {
				var toRemove = new List<ModMeta>();
				var removeFromMods = new List<ModMeta>();
				foreach(var mod in waiting) {
					// if deps are now loaded, load and remove from waiting list
					bool willLoad = true;
					foreach(var dep in mod.Dependencies)
						if(!Contains(dep, loaded))
							willLoad = false;
					foreach(var dep in mod.OptionalDependencies)
						if(!Contains(dep, loaded))
							willLoad = false;
					if(willLoad) {
						LoadModFromMeta(mod);
						toRemove.Add(mod);
					} else {
						// check if a dependency has missing deps itself
						bool wontLoad = false;
						foreach(var dep in mod.Dependencies)
							if(!Contains(dep, Mods))
								wontLoad = true;
						if(wontLoad) {
							Logger.Log("Not loading " + mod.Name + ": missing dependency.");
							removeFromMods.Add(mod);
						} else {
							// check that outdated optional dependencies still exist
							bool skipLoad = true;
							foreach(var opDep in mod.OptionalDependencies)
								if(Mods.Any(m => m.Name.Equals(opDep.Name) && m.Version < opDep.Version))
									skipLoad = false;
							if(skipLoad) {
								LoadModFromMeta(mod);
								toRemove.Add(mod);
							}
						}
					}
				}
				// if we don't load any mods, we have a circular dep, don't load any more
				waiting.RemoveAll(m => toRemove.Contains(m));
				Mods.RemoveAll(m => removeFromMods.Contains(m));
				if(toRemove.Count() == 0) {
					foreach(var item in waiting)
						Logger.Log("Not loading " + item.Name + ": circular dependency!");
					break;
				}
			}

			// Add mod content
			// Load mods
			foreach(var mod in CodeMods)
				mod.Load();
			Logger.Log($"Finished pre-init loading - {Mods.Count} mods loaded; {CodeMods.Count} assemblies, {ModContentDirectories.Count} content directories, and {ModCampaignModels.Count} custom campaigns found.");
		} catch(Exception e) {
			if(Logger.Setup) {
				Logger.Log("Failed to pre-initialize!");
				Logger.Log(e);
			}
			throw;
		}
	}

	private static void LoadModFromMeta(ModMeta mod) {
		if(mod == QuintessentialModMeta)
			return;
		if(!string.IsNullOrWhiteSpace(mod.DLL)) {
			string dllPath = mod.DLL;
			LoadModAssembly(mod, GetRemappedAssembly(dllPath, mod));
		}
		// Get mod content
		//  - Consider modded folders when fetching any content
		//  - Custom language files: vanilla stores in a big CSV, but for custom dialogue (and languages) we'll want seperate files (e.g. English.txt, French.txt)
		//  - Custom solitaires too?
		var content = Path.Combine(mod.PathDirectory, "Content");
		if(Directory.Exists(content))
			ModContentDirectories.Add(mod.PathDirectory);

		var puzzles = Path.Combine(mod.PathDirectory, "Puzzles");
		if(Directory.Exists(puzzles)) {
			ModPuzzleDirectories.Add(puzzles);
			LoadModPuzzlesFromMeta(puzzles);
		};

		loaded.Add(mod);
		Logger.Log($"Will load mod \"{mod.Name}\".");
	}

	public static void PostLoad() {
		Logger.Log("Starting post-init loading.");
		// Read mod save data
		PathModSaves = Path.Combine(class_161.method_402(), "ModSettings");
		Logger.Log($"Mod settings directory: \"{PathModSaves}\"");
		if(!Directory.Exists(PathModSaves))
			Directory.CreateDirectory(PathModSaves);
		foreach(var mod in CodeMods) {
			var savePath = Path.Combine(PathModSaves, mod.Meta.Name + ".yaml");
			if(File.Exists(savePath)) {
				using StreamReader reader = new(savePath);

				var settings = YamlHelper.Deserializer.Deserialize(reader, mod.SettingsType);
				if(settings != null) {
					mod.Settings = settings;
					mod.ApplySettings();
				} else
					Logger.Log("Loaded null settings for mod " + mod.Meta.Name);
			}
		}
		foreach(var mod in CodeMods)
			mod.PostLoad();
		Logger.Log("Finished post-init loading.");
	}

	public static void LoadPuzzleContent() {
		Logger.Log("Starting puzzle content loading.");
		foreach(var mod in CodeMods)
			mod.LoadPuzzleContent();

		// debug
		QApi.AddPuzzlePermission("MyCoolPart", "Glyph of Coolness");
		QApi.AddPuzzlePermission("GlyphIrridation", "Glyph of Irridation");
		QApi.AddPuzzlePermission("ArmScissor", "Scissor Arms");

		Logger.Log("Finished puzzle content loading.");
	}

	public static void Unload() {
		Logger.Log("Starting mod unloading.");
		foreach(var mod in CodeMods)
			mod.Unload();
		Logger.Log("Finished unloading.");
	}
	#endregion

	#region Misc Helper Methods
	protected static void FindZipMod(string zip) {
		Logger.Log("Unzipping zip mod: " + zip);
		// Check that the zip exists
		if(!File.Exists(zip)) // Relative path?
			zip = Path.Combine(PathMods, zip);
		if(!File.Exists(zip)) // It just doesn't exist.
			return;

		var dest = zip.Substring(0, zip.Length - ".zip".Length) + zipExtractSuffix;
		using(ZipFile file = new(zip))
			file.ExtractAll(dest);
		FindFolderMod(dest, zip);
	}

	protected static void FindFolderMod(string dir, string zipName = null) {
		// don't load zip mods again, ignore quintessential assets
		if(dir.EndsWith(quintAssetFolder) || (string.IsNullOrEmpty(zipName) && dir.EndsWith(zipExtractSuffix)))
			return;

		Logger.Log($"Finding mod in folder: \"{dir}\"");
		// Check that the folder exists
		if(!Directory.Exists(dir)) // Relative path?
			dir = Path.Combine(PathMods, dir);
		if(!Directory.Exists(dir)) // It just doesn't exist.
			return;

		// Look for a mod meta
		ModMeta meta;
		string metaPath = Path.Combine(dir, "quintessential.yaml");
		if(!File.Exists(metaPath))
			metaPath = Path.Combine(dir, "quintessential.yml");
		if(File.Exists(metaPath)) {
			using StreamReader reader = new(metaPath);

			try {
				if(!reader.EndOfStream) {
					meta = YamlHelper.Deserializer.Deserialize<ModMeta>(reader);
					meta.Name = meta.Name.Trim().Replace(" ", "_");
					meta.PathDirectory = dir;
					if(!string.IsNullOrEmpty(zipName))
						meta.PathArchive = zipName;
					meta.PostParse();
					Mods.Add(meta);
					Logger.Log($"Queuing mod \"{meta.Name}\", version {meta.VersionString}.");
				}
			} catch(Exception e) {
				Logger.Log($"Failed parsing quintessential.yaml in {dir}: {e}");
			}
		} else {
			meta = new ModMeta {
				Name = "NoMetaMod__" + Path.GetFileName(dir),
				PathDirectory = dir
			};
			if(!string.IsNullOrEmpty(zipName))
				meta.PathArchive = zipName;
			meta.PostParse();
			Mods.Add(meta);
			Logger.Log($"Will load mod without metadata from \"{dir}\".");
		}
	}

	protected static void LoadModAssembly(ModMeta meta, Assembly asm) {
		Type[] types;
		try {
			try {
				types = asm.GetTypes();
			} catch(ReflectionTypeLoadException e) {
				types = e.Types.Where(t => t != null).ToArray();
			}
		} catch(Exception e) {
			Logger.Log($"Failed reading assembly for {meta.Name}: {e}");
			e.LogDetailed();
			return;
		}

		for(int i = 0; i < types.Length; i++) {
			Type type = types[i];

			if(typeof(QuintessentialMod).IsAssignableFrom(type) && !type.IsAbstract) {
				QuintessentialMod mod = (QuintessentialMod)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
				mod.Meta = meta;
				Register(mod);
			}
		}
	}

	protected static void CleanupExtractedData() {
		string[] folders = Directory.GetDirectories(PathMods);
		foreach(var folder in folders)
			if(folder.EndsWith(zipExtractSuffix) || folder.EndsWith(quintAssetFolder))
				Directory.Delete(folder, true);
	}

	protected static void Register(QuintessentialMod mod) {
		CodeMods.Add(mod);
	}

	public static Assembly GetRemappedAssembly(string asmPath, ModMeta meta) {
		if(!string.IsNullOrEmpty(meta.Mappings)) {
			// load mappings
			// load assembly def
			// remap
			// save in cache
			// load that
			//OpusMutatum.OpusMutatum.DoRemap();
		}
		return Assembly.LoadFrom(asmPath);
	}
	#endregion

	#region methods related to Custom Campaigns
	public static List<Campaign> AllCampaigns = new();
	public static Campaign VanillaCampaign;
	private static List<CampaignModel> ModCampaignModels = new();
	private static List<JournalModel> ModJournalModels = new();
	private static List<SoundModel> ModSounds = new();
	private static List<SongModel> ModSongs = new();
	private static List<VignetteActorModel> ModVignetteActors = new();
	private static List<LocationModel> ModLocations = new();
	private static List<DocumentModel> ModDocuments = new();

	public static void LoadSounds()
	{
		foreach (var s in ModSounds)
		{
			QApi.loadSound(s.Path, s.Volume);
			Logger.Log($"  Added sound \"{s.Path}\"");
		}
	}
	public static void LoadSongs()
	{
		foreach (var s in ModSongs)
		{
			QApi.loadSong(s.Path);
			Logger.Log($"  Added song \"{s.Path}\"");
		}
	}
	public static void LoadVignetteActors()
	{
		foreach (var a in ModVignetteActors)
		{
			Texture smallPortrait = null;
			Texture largePortrait = null;
			if (!string.IsNullOrEmpty(a.SmallPortrait))
			{
				smallPortrait = QApi.loadTexture(a.SmallPortrait);
			}
			if (!string.IsNullOrEmpty(a.LargePortrait))
			{
				largePortrait = QApi.loadTexture(a.LargePortrait);
			}
			QApi.addVignetteActor(a.ID, a.Name, Color.FromHex(a.Color), smallPortrait, largePortrait, a.IsOnLeft);
			Logger.Log($"  Added actor \"{a.Name}\"");
		}
	}
	public static void LoadLocations()
	{
		foreach (var l in ModLocations)
		{
			QApi.loadTexture(l.Path);
			Logger.Log($"  Added background texture \"{l.Path}\"");
		}
	}
	public static void LoadDocuments()
	{
		foreach (var d in ModDocuments)
		{
			Texture base_texture = null;
			List<patch_DocumentScreen.DrawItem> drawItems = new();

			if (!string.IsNullOrEmpty(d.Texture))
			{
				base_texture = QApi.loadTexture(d.Texture);
			}
			else
			{
				Logger.Log($"Document \"{d.ID}\" doesn't have a base texture defined, ignoring");
			}
			int maxIndex = d.DrawItems == null ? 0 : d.DrawItems.Count;
			for (int i = 0; i < maxIndex; i++)
			{
				var item = d.DrawItems[i];
				Vector2 position = new Vector2(0f, 0f);
				Font font = class_238.field_1990.field_2150;
				Color color = patch_DocumentScreen.field_2410;
				enum_0 alignment = (enum_0)0;
				float lineSpacing = 1f;
				float columnWidth = float.MaxValue;

				if (!string.IsNullOrEmpty(item.Position))
				{
					float x, y;
					string pos = item.Position;

					if (float.TryParse(pos.Split(',')[0], out x) && float.TryParse(pos.Split(',')[1], out y))
					{
						position = new Vector2(x, y);
					}
				}

				if (!string.IsNullOrEmpty(item.Texture))
				{
					//make graphic item
					drawItems.Add(new patch_DocumentScreen.DrawItem(position, QApi.loadTexture(item.Texture)));
				}
				else
				{
					//make a text item
					if (!string.IsNullOrEmpty(item.Font))
					{
						font = QApi.getFont(item.Font);
					}
					if (!string.IsNullOrEmpty(item.Color))
					{
						color = Color.FromHex(int.Parse(item.Color));
					}
					if (!string.IsNullOrEmpty(item.Align))
					{
						if (item.Align.ToLower() == "center")
						{
							alignment = (enum_0)1;
						}
						else if (item.Align.ToLower() == "right")
						{
							alignment = (enum_0)2;
						}
					}
					if (!string.IsNullOrEmpty(item.LineSpacing))
					{
						lineSpacing = float.Parse(item.LineSpacing);
					}
					if (!string.IsNullOrEmpty(item.ColumnWidth))
					{
						columnWidth = float.Parse(item.ColumnWidth);
					}
					drawItems.Add(new patch_DocumentScreen.DrawItem(position, font, color, alignment, lineSpacing, columnWidth, item.Handwritten));
				}
			}
			patch_DocumentScreen.createSimpleDocument(d.ID, base_texture, drawItems);
			Logger.Log($"  Added document \"{d.ID}\"");
		}
	}
	private static void LoadModPuzzlesFromMeta(string puzzles)
	{
		//helper function
		string AddS(int amount)
		{
			return amount == 1 ? "" : "s";
		}
		// Look for name.campaign.yaml and name.journal.yaml files in the folder
		foreach (var item in Directory.GetFiles(puzzles))
		{
			string filename = Path.GetFileName(item);
			Logger.Log("    Looking at " + filename);
			using StreamReader reader = new(item);

			if (filename.EndsWith(".resources.yaml"))
			{
				ResourcesModel c = YamlHelper.Deserializer.Deserialize<ResourcesModel>(reader);
				if (c.Characters.Count > 0)
				{
					Logger.Log($"    Adding {c.Characters.Count} vignette actor{AddS(c.Characters.Count)}.");
					ModVignetteActors.AddRange(c.Characters);
				}
				if (c.Sounds.Count > 0)
				{
					Logger.Log($"    Adding {c.Sounds.Count} sound{AddS(c.Sounds.Count)}.");
					ModSounds.AddRange(c.Sounds);
				}
				if (c.Songs.Count > 0)
				{
					Logger.Log($"    Adding {c.Songs.Count} song{AddS(c.Songs.Count)}.");
					ModSongs.AddRange(c.Songs);
				}
				if (c.Locations.Count > 0)
				{
					Logger.Log($"    Adding {c.Locations.Count} cutscene location{AddS(c.Locations.Count)}.");
					ModLocations.AddRange(c.Locations);
				}
				if (c.Documents.Count > 0)
				{
					Logger.Log($"    Adding {c.Documents.Count} documents{AddS(c.Documents.Count)}.");
					ModDocuments.AddRange(c.Documents);
				}

			}
			if (filename.EndsWith(".campaign.yaml"))
			{
				CampaignModel c = YamlHelper.Deserializer.Deserialize<CampaignModel>(reader);
				Logger.Log($"Campaign \"{c.Title}\" ({c.Name}) has {c.Chapters.Count} chapter{AddS(c.Chapters.Count)}.");
				c.Path = Path.GetDirectoryName(item);
				ModCampaignModels.Add(c);
			}
			if (filename.EndsWith(".journal.yaml"))
			{
				JournalModel c = YamlHelper.Deserializer.Deserialize<JournalModel>(reader);
				Logger.Log($"Journal \"{c.Title}\" has {c.Chapters.Count} chapter{AddS(c.Chapters.Count)}.");
				bool valid = true;
				foreach (var chapter in c.Chapters)
				{
					if (chapter.Puzzles.Count != 5)
					{
						Logger.Log($"Journal chapter \"{chapter.Title}\" in \"{c.Title}\" doesn't have 5 puzzles; skipping journal.");
						valid = false; break;
					}
				}
				if (valid)
				{
					c.Path = Path.GetDirectoryName(item);
					ModJournalModels.Add(c);
				}
			}
		}
	}
	public static void LoadCampaigns() {
		VanillaCampaign = Campaigns.field_2330;
		((patch_Campaign)(object)VanillaCampaign).QuintTitle = "Opus Magnum";
		((patch_Campaign)(object)VanillaCampaign).Music = "music/Map";
		((patch_Campaign)(object)VanillaCampaign).ButtonBase = "textures/puzzle_select/chapter_base";

		AllCampaigns.Add(VanillaCampaign);

		for(int i = 0; i < ModCampaignModels.Count; i++) {
			CampaignModel c = ModCampaignModels[i];
			var campaign = new Campaign {
				field_2309 = new CampaignChapter[c.Chapters.Count]
			};

			((patch_Campaign)(object)campaign).QuintTitle = c.Title;
			((patch_Campaign)(object)campaign).Music = !string.IsNullOrEmpty(c.Music) ? c.Music : ((patch_Campaign)(object)Campaigns.field_2330).Music;
			((patch_Campaign)(object)campaign).ButtonBase = !string.IsNullOrEmpty(c.ButtonBase) ? c.ButtonBase : ((patch_Campaign)(object)Campaigns.field_2330).ButtonBase;


			for (int j = 0; j < c.Chapters.Count; j++) {
				ChapterModel chapter = c.Chapters[j];

				Texture bLocked = Campaigns.field_2330.field_2309[j].field_2316;
				Texture bUnlocked = Campaigns.field_2330.field_2309[j].field_2317;
				Texture bHover = Campaigns.field_2330.field_2309[j].field_2318;
				Texture bGem = Campaigns.field_2330.field_2309[j].field_2319;
				Vector2 bPosition = Campaigns.field_2330.field_2309[j].field_2320;
				if (chapter.Button != null)
				{
					//helper function
					Texture SafeLoadTexture(string path, Texture tex)
					{
						if (!string.IsNullOrEmpty(path))
						{
							tex = QApi.loadTexture(path);
						}
						return tex;
					}
					//attempt to load button textures
					bLocked = SafeLoadTexture(chapter.Button.Locked, bLocked);
					bUnlocked = SafeLoadTexture(chapter.Button.Unlocked, bUnlocked);
					bHover = SafeLoadTexture(chapter.Button.Hover, bHover);
					bGem = SafeLoadTexture(chapter.Button.Gem, bGem);
					//attempt to load button position
					if (!string.IsNullOrEmpty(chapter.Button.Position))
					{
						float x, y;
						string pos = chapter.Button.Position;
						if (float.TryParse(pos.Split(',')[0], out x) && float.TryParse(pos.Split(',')[1], out y))
						{
							bPosition = new Vector2(x, y);
						}
						else
						{
							Logger.Log($"Can't parse \"Button.Position\" in chapter {j} from campaign \"{c.Title}\", ignoring");
						}
					}
				}

				campaign.field_2309[j] = new CampaignChapter(
					class_134.method_253(chapter.Title, string.Empty),
					class_134.method_253(chapter.Subtitle, string.Empty),
					class_134.method_253(chapter.Place, string.Empty),
					chapter.Background != null ? class_235.method_615(chapter.Background) : Campaigns.field_2330.field_2309[j].field_2315,
					bLocked,
					bUnlocked,
					bHover,
					bGem,
					bPosition,
					chapter.IsLeftSide
				);

				foreach (var entry in chapter.Entries)
				{
					string baseName;

					//determine entry type, then load relevant data
					enum_129 entryType;
					Maybe<Puzzle> maybePuzzle = struct_18.field_1431;


					if (!string.IsNullOrEmpty(entry.Solitaire))
					{
						entryType = (enum_129)3;
					}
					else if (entry.Document)
					{
						entryType = (enum_129)2;
					}
					else if (!string.IsNullOrEmpty(entry.Puzzle))
					{
						baseName = Path.Combine(c.Path, entry.Puzzle);
						entryType = 0;
						Puzzle puzzle;
						if (File.Exists(baseName + ".puzzle"))
						{
							puzzle = Puzzle.method_1249(baseName + ".puzzle");
						}
						else if (File.Exists(baseName + ".puzzle.yaml"))
						{
							puzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(baseName + ".puzzle.yaml")));
						}
						else
						{
							Logger.Log($"Puzzle \"{entry.Puzzle}\" from campaign \"{c.Title}\" doesn't exist, ignoring");
							continue;
						}
						puzzle.field_2766 = entry.ID;
						// ensure all inputs/outputs have names
						foreach (PuzzleInputOutput io in puzzle.field_2770.Union(puzzle.field_2771))
						{
							if (!io.field_2813.field_2639.method_1085())
							{
								io.field_2813.field_2639 = class_134.method_253("Molecule", string.Empty);
							}
						}
						maybePuzzle = puzzle;
					}
					else if (entry.Cutscene != null)
					{
						entryType = (enum_129)1;
						patch_CutsceneScreen.addCutsceneData(entry.ID, entry.Cutscene.Location, entry.Cutscene.Background, entry.Cutscene.SlowFade);
					}
					else
					{
						Logger.Log($"Invalid entry \"{entry.ID}\" from campaign \"{c.Title}\", ignoring");
						continue;
					}
					// implement other entry types later, and modify other code so we can do solitaires, letters, etc. more easily

					Maybe<class_215> tutorialScreen = struct_18.field_1431; // implement later - refer to method_558
					string songPath = string.IsNullOrEmpty(entry.Song) ? "music/Solving3" : entry.Song;
					string fanfarePath = string.IsNullOrEmpty(entry.Fanfare) ? "sounds/fanfare_solving3" : entry.Fanfare;

					class_259 requirement;
					int n;
					if (string.IsNullOrEmpty(entry.Requires))
					{
						requirement = new class_174();
					}
					else if (int.TryParse(entry.Requires, out n))
					{
						requirement = new class_265(n);
					}
					else
					{
						requirement = new class_243(entry.Requires);
					}
					/*
					entryType = (enum_129)2
					*/

					// TODO: optimize

					AddEntryToCampaign(campaign, j, entry.ID, class_134.method_253(entry.Title, string.Empty), entryType, tutorialScreen, maybePuzzle, QApi.loadSong(songPath), QApi.loadSound(fanfarePath), requirement);
					if (maybePuzzle.method_1085())
					{
						Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
						Puzzles.field_2816[Puzzles.field_2816.Length - 1] = maybePuzzle.method_1087();
					}

				}
			}

			for(int index = 0; index < campaign.field_2309.Length; ++index)
				campaign.field_2309[index].field_2310 = index;

			AllCampaigns.Add(campaign);
		}
	}

	private static void AddEntryToCampaign(
			Campaign campaign,
			int chapter,
			string entryId,
			LocString entryTitle,
			enum_129 type,
			Maybe<class_215> param_4485,
			Maybe<Puzzle> puzzle,
			class_186 param_4487,
			Sound clickSound,
			class_259 requirement) {
		if(puzzle.method_1085()) {
			//puzzle.method_1087().field_2767 = entryTitle;
			puzzle.method_1087().field_2769 = param_4485;
		}
		CampaignItem campaignItem = new(entryId, entryTitle, type, puzzle, requirement, param_4487, clickSound);
		campaign.field_2309[chapter].field_2314.Add(campaignItem);
	}

	internal static void DumpVanillaPuzzles() {
		string outDir = Path.Combine(PathModSaves, "Quintessential", "DumpedPuzzles");
		Directory.CreateDirectory(outDir);
		foreach(var p in Puzzles.field_2816) {
			PuzzleModel m = PuzzleModel.FromPuzzle(p);
			string yaml = YamlHelper.Serializer.Serialize(m);
			File.WriteAllText(Path.Combine(outDir, m.ID + ".yaml"), yaml);
		}
		foreach(var p in JournalVolumes.field_2572.SelectMany(k => k.field_2571)) {
			PuzzleModel m = PuzzleModel.FromPuzzle(p);
			string yaml = YamlHelper.Serializer.Serialize(m);
			File.WriteAllText(Path.Combine(outDir, "X" + m.ID + ".yaml"), yaml);
		}
		Logger.Log($"Dumped puzzles to {outDir}");
		UI.OpenScreen(new NoticeScreen("Puzzle Dumping", $"Saved puzzles to \"{outDir.Replace('\\', '/')}\""));
	}
	#endregion
}
