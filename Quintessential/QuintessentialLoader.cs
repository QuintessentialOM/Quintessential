using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using Ionic.Zip;

using MonoMod.Utils;

using Quintessential.Serialization;

namespace Quintessential;

public class QuintessentialLoader {

	public static readonly string VersionString = "0.4.0";
	public static readonly int VersionNumber = 7;

	public static string PathLightning;
	public static string PathMods;
	public static string PathBlacklist;
	public static string PathModSaves;
	public static string PathScreenshots;

	public static List<QuintessentialMod> CodeMods = new();
	public static List<ModMeta> Mods = new();
	public static List<string> ModContentDirectories = new();
	public static List<string> ModPuzzleDirectories = new();

	public static List<Campaign> AllCampaigns = new();
	public static Campaign VanillaCampaign;
	public static List<List<JournalVolume>> AllJournals = new();
	public static List<JournalVolume> VanillaJournal;

	public static ModMeta QuintessentialModMeta;
	public static QuintessentialMod QuintessentialAsMod;

	public static List<CampaignModel> ModCampaignModels = new();
	public static List<JournalModel> ModJournalModels = new();

	private static List<string> blacklisted = new();
	private static List<ModMeta> loaded = new();
	private static List<ModMeta> waiting = new();

	private static readonly string zipExtractSuffix = "__quintessential_from_zip";
	private static readonly string quintAssetFolder = "__quintessential_assets";

	public static void PreInit() {
		try {
			PathLightning = Path.GetDirectoryName(typeof(GameLogic).Assembly.Location);
			PathMods = Path.Combine(PathLightning, "Mods");
			PathScreenshots = Path.Combine(PathLightning, "Screenshots");

			Logger.Init();
			Logger.Log($"Quintessential v{VersionString} ({VersionNumber})");
			Logger.Log("Starting pre-init loading.");

			QApi.Init();

			if(!Directory.Exists(PathMods))
				Directory.CreateDirectory(PathMods);
			if(!Directory.Exists(PathScreenshots))
				Directory.CreateDirectory(PathScreenshots);

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

			while(waiting.Any()){
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
				if(!toRemove.Any()) {
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

		LoadModCampaigns(mod);

		loaded.Add(mod);
		Logger.Log($"Will load mod \"{mod.Name}\".");
	}

	private static void LoadModCampaigns(ModMeta mod){
		var puzzles = Path.Combine(mod.PathDirectory, "Puzzles");
		if(Directory.Exists(puzzles)){
			if(!ModPuzzleDirectories.Contains(puzzles))
				ModPuzzleDirectories.Add(puzzles);
			// Look for name.campaign.yaml and name.journal.yaml files in the folder
			foreach(var item in Directory.GetFiles(puzzles)){
				string filename = Path.GetFileName(item);
				if(filename.EndsWith(".campaign.yaml")){
					using StreamReader reader = new(item);

					CampaignModel c = YamlHelper.Deserializer.Deserialize<CampaignModel>(reader);
					Logger.Log($"Campaign \"{c.Title}\" ({c.Name}) has {c.Chapters.Count} chapters.");
					c.Path = Path.GetDirectoryName(item);
					ModCampaignModels.Add(c);
				}

				if(filename.EndsWith(".journal.yaml")){
					using StreamReader reader = new(item);

					JournalModel c = YamlHelper.Deserializer.Deserialize<JournalModel>(reader);
					Logger.Log($"Journal \"{c.Title}\" has {c.Chapters.Count} chapters.");
					foreach(var chapter in new List<JournalChapterModel>(c.Chapters)){
						if(chapter.Puzzles.Count != 5){
							Logger.Log($"Journal chapter \"{chapter.Title}\" in \"{c.Title}\" has {chapter.Puzzles.Count} puzzles instead of 5; skipping chapter.");
							c.Chapters.Remove(chapter);
						}
					}

					if(c.Chapters.Count > 0){
						c.Path = Path.GetDirectoryName(item);
						ModJournalModels.Add(c);
					}else
						Logger.Log($"Journal \"{c.Title}\" has no chapters, skipping.");
				}
			}
		}
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

		Logger.Log("Loading campaigns and journals.");
		LoadCampaigns();
		LoadJournals();
		
		Logger.Log("Finished puzzle content loading.");
	}

	public static void Unload() {
		Logger.Log("Starting mod unloading.");
		foreach(var mod in CodeMods)
			mod.Unload();
		Logger.Log("Finished unloading.");
	}

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

	public static void LoadCampaigns(){
		AllCampaigns.Clear();

		VanillaCampaign = Campaigns.field_2330;
		((patch_Campaign)(object)VanillaCampaign).QuintTitle = "Opus Magnum";
		AllCampaigns.Add(VanillaCampaign);

		foreach(var c in ModCampaignModels){
			var campaign = new Campaign{
				field_2309 = new CampaignChapter[c.Chapters.Count]
			};

			((patch_Campaign)(object)campaign).QuintTitle = c.Title;

			for(int j = 0; j < c.Chapters.Count; j++) {
				ChapterModel chapter = c.Chapters[j];
				campaign.field_2309[j] = new CampaignChapter(
					class_134.method_253(chapter.Title, string.Empty),
					class_134.method_253(chapter.Subtitle, string.Empty),
					class_134.method_253(chapter.Place, string.Empty),
					chapter.Background != null ? class_235.method_615(chapter.Background) : Campaigns.field_2330.field_2309[j].field_2315,
					Campaigns.field_2330.field_2309[j].field_2316,
					Campaigns.field_2330.field_2309[j].field_2317,
					Campaigns.field_2330.field_2309[j].field_2318,
					Campaigns.field_2330.field_2309[j].field_2319,
					Campaigns.field_2330.field_2309[j].field_2320,
					false
				);

				foreach(var entry in chapter.Entries){
					class_259 requirement = string.IsNullOrEmpty(entry.Requires) ? (class_259)new class_174() : new class_243(entry.Requires);

					var lower = entry.Type.ToLowerInvariant();
					CampaignItem cItem;
					switch(lower){
						case "puzzle": {
							if(!TryLoadPuzzle(c.Path, entry.Puzzle, c.Title, out var puzzle))
								continue;

							puzzle.field_2766 = entry.ID;
							// ensure all inputs/outputs have names
							foreach(PuzzleInputOutput io in puzzle.field_2770.Union(puzzle.field_2771)){
								if(!io.field_2813.field_2639.method_1085()){
									io.field_2813.field_2639 = class_134.method_253("Molecule", string.Empty);
								}
							}

							// TODO: optimize
							cItem = AddEntryToCampaign(campaign, j, entry.ID, class_134.method_253(entry.Title, string.Empty), (enum_129)0, struct_18.field_1431, puzzle, class_238.field_1992.field_972, class_238.field_1991.field_1832, requirement);
							Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
							Puzzles.field_2816[Puzzles.field_2816.Length - 1] = puzzle;
							break;
						}
						case "solitaire": {
							cItem = new(entry.ID, class_134.method_253("Sigmar's Garden", string.Empty), (enum_129) 3, struct_18.field_1431, requirement, class_238.field_1992.field_970, class_238.field_1991.field_1830);
							campaign.field_2309[j].field_2314.Add(cItem);
							break;
						}
						default:
							Logger.Log($"Campaign entry in {c.Name} has unknown type {entry.Type}, skipping");
							continue;
					}

					patch_CampaignItem conv = (patch_CampaignItem)(object)cItem;
					// probably not great to reload the images every time, in the case that a campaign uses the same image on every puzzle?
					// but these are small, and we can definitely handle the case where every puzzle has a unique icon
					if(!string.IsNullOrWhiteSpace(entry.Icon))
						conv.Icon = class_235.method_615(entry.Icon);
					if(!string.IsNullOrWhiteSpace(entry.IconSmall))
						conv.IconSmall = class_235.method_615(entry.IconSmall);
				}
			}

			for(int index = 0; index < campaign.field_2309.Length; ++index)
				campaign.field_2309[index].field_2310 = index;

			AllCampaigns.Add(campaign);
		}
	}

	public static void LoadJournals(){
		AllJournals.Clear();
		
		VanillaJournal = JournalVolumes.field_2572.ToList();
		AllJournals.Add(VanillaJournal);
		
		foreach(JournalModel journal in ModJournalModels){
			List<JournalVolume> volumes = journal.Chapters.Select(chapter =>
				new JournalVolume{
					field_2569 = chapter.Title,
					field_2570 = chapter.Description,
					field_2571 = chapter.Puzzles.SelectMany(puzzleName =>
						TryLoadPuzzle(journal.Path, puzzleName, journal.Title, out var puzzle) ? new[]{ puzzle } : new Puzzle[0]).ToArray()
				}).ToList();
			foreach(JournalVolume jv in volumes){
				Logger.Log($"Journal {jv.field_2569} has {jv.field_2571.Length} puzzles");
			}
			AllJournals.Add(volumes);
		}
	}

	private static bool TryLoadPuzzle(string basePath, string puzzleName, string campaignTitle, out Puzzle puzzle){
		try{
			string baseName = Path.Combine(basePath, puzzleName);
			if(File.Exists(baseName + ".puzzle")){
				puzzle = Puzzle.method_1249(baseName + ".puzzle");
			}else if(File.Exists(baseName + ".puzzle.yaml")){
				puzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(baseName + ".puzzle.yaml")));
			}else{
				Logger.Log($"Puzzle \"{puzzleName}\" from \"{campaignTitle}\" doesn't exist, ignoring");
				puzzle = null;
				return false;
			}
			
			// even if it was loaded from a vanilla format puzzle file, it was included in a mod and may rely on modded behaviour
			// these are never saved over and could have been modified directly by the campaign mod, so this is safe
			((patch_Puzzle)(object)puzzle).IsModdedPuzzle = true;

			return true;
		}catch(Exception e){
			Logger.Log($"Exception loading puzzle \"{puzzleName}\" from \"{campaignTitle}\", ignoring");
			Logger.Log(e);
			puzzle = null;
			return false;
		}
	}

	public static void CheckCampaignReload(){
		if(QuintessentialSettings.Instance.HotReloadCampaigns.Pressed() && GameLogic.field_2434.method_938() is PuzzleSelectScreen){
			Logger.Log("Reloading campaigns and journals!");
			
			ModPuzzleDirectories.Clear();
			ModCampaignModels.Clear();
			ModJournalModels.Clear();
			
			Campaigns.field_2330 = VanillaCampaign;
			Campaigns.field_2331[0] = VanillaCampaign;
			JournalVolumes.field_2572 = VanillaJournal.ToArray();
			patch_PuzzleSelectScreen.ResetPosition();
			patch_JournalScreen.ResetPosition();
			
			foreach(ModMeta mod in Mods)
				if(mod != QuintessentialModMeta)
					LoadModCampaigns(mod);
			
			LoadCampaigns();
			LoadJournals();
			UI.InstantCloseScreen();
			UI.OpenScreen(new PuzzleSelectScreen());
		}
	}

	private static CampaignItem AddEntryToCampaign(
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
		return campaignItem;
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

	internal static void DumpAtomSprites(){
		string outDir = Path.Combine(PathModSaves, "Quintessential", "DumpedAtomSprites");
		Directory.CreateDirectory(outDir);
		foreach(AtomType atomType in class_175.field_1691){
			RenderTargetHandle v = RenderAtomToTarget(atomType);
			Renderer.method_1313(v.method_1351().field_937).method_735(Path.Combine(outDir, ((patch_AtomType)(object)atomType).QuintAtomType.Replace(":", "_") + ".png"));
		}
		Logger.Log($"Dumped atom sprites to {outDir}");
		UI.OpenScreen(new NoticeScreen("Sprite Dumping", $"Saved atom sprites to \"{outDir.Replace('\\', '/')}\""));
	}

	internal static RenderTargetHandle RenderAtomToTarget(AtomType type){
		RenderTargetHandle renderTargetHandle = new RenderTargetHandle();
		Bounds2 bounds = Bounds2.CenteredOn(class_187.field_1742.method_491(new HexIndex(0, 0), Vector2.Zero), class_187.field_1742.field_1744.X, class_187.field_1742.field_1744.Y * 1.3f);
		Index2 size = bounds.Size.CeilingToInt() + new Index2(20 * 2, 20 * 2);
		Vector2 pos = size.ToVector2() / 2 / 1f - bounds.Center;
		pos.Y = -pos.Y;
		renderTargetHandle.field_2987 = size;
		class_95 class95 = renderTargetHandle.method_1352(out var flag);
		if(flag){
			using(class_226.method_597(class95, Matrix4.method_1074(new Vector3(1, -1, 1)))){
				class_226.method_600(Color.Transparent);
				Editor.method_927(type, pos, 1, 1, 1, 1, -21, 0, class_238.field_1989.field_71, null, false);
			}
		}

		return renderTargetHandle;
	}
}