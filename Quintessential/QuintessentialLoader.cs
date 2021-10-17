using Ionic.Zip;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quintessential {

	public class QuintessentialLoader {

		public readonly static string VersionString = "0.2.0";
		public readonly static int VersionNumber = 5;

		public static string PathLightning;
		public static string PathMods;
		public static string PathBlacklist;
		public static string PathModSaves;

		public static List<QuintessentialMod> CodeMods = new List<QuintessentialMod>();
		public static List<ModMeta> Mods = new List<ModMeta>();
		public static List<string> ModContentDirectories = new List<string>();

		public static ModMeta QuintessentialModMeta;

		private static List<string> blacklisted = new List<string>();
		private static List<ModMeta> loaded = new List<ModMeta>();
		private static List<ModMeta> waiting = new List<ModMeta>();

		private static readonly string zipExtractSuffix = "__quintessential_from_zip";

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
				// Delete leftover quintessential extracted zips
				CleanupExtractedZips();

				// Add Quintessential mod & mod meta
				QuintessentialModMeta = new ModMeta {
					Name = "Quintessential",
					Version = new Version(VersionString)
				};
				Mods.Add(QuintessentialModMeta);
				var asMod = new Internal.QuintessentialAsMod();
				asMod.Meta = QuintessentialModMeta;
				asMod.Settings = new QuintessentialSettings();
				CodeMods.Add(asMod);

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
				Func<ModMeta.Dependency, List<ModMeta>, bool> Contains = (dep, list) => list.Any(m => m.Name.Equals(dep.Name) && m.Version >= dep.Version);
				List<ModMeta> rem = new List<ModMeta>();
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
					Logger.Log($"Finished pre-init loading - {Mods.Count} mods loaded, {CodeMods.Count} assemblies loaded, {ModContentDirectories.Count} content directories found.");
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
			loaded.Add(mod);
			Logger.Log("Will load mod " + mod.Name + ".");
		}

		public static void PostLoad() {
			Logger.Log("Starting post-init loading.");
			// Read mod save data
			PathModSaves = Path.Combine(class_161.method_402(), "ModSettings");
			Logger.Log("Mod settings directory: " + PathModSaves);
			if(!Directory.Exists(PathModSaves))
				Directory.CreateDirectory(PathModSaves);
			foreach(var mod in CodeMods) {
				var savePath = Path.Combine(PathModSaves, mod.Meta.Name + ".yaml");
				if(File.Exists(savePath)) {
					using(StreamReader reader = new StreamReader(savePath)) {
						var settings = YamlHelper.Deserializer.Deserialize(reader, mod.SettingsType);
						if(settings != null) {
							mod.Settings = settings;
							mod.ApplySettings();
						} else
							Logger.Log("Loaded null settings for mod " + mod.Meta.Name);
					}
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
			using(ZipFile file = new ZipFile(zip))
				file.ExtractAll(dest);
			FindFolderMod(dest, zip);
		}

		protected static void FindFolderMod(string dir, string zipName = null) {
			// don't load zip mods again
			if(string.IsNullOrEmpty(zipName) && dir.EndsWith("__quintessential_from_zip"))
				return;
			
			Logger.Log("Finding mod in folder: " + dir);
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
				using(StreamReader reader = new StreamReader(metaPath)) {
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
				}
			} else {
				meta = new ModMeta();
				meta.Name = "NoMetaMod_" + Path.GetFileName(dir);
				meta.PathDirectory = dir;
				if(!string.IsNullOrEmpty(zipName))
					meta.PathArchive = zipName;
				meta.PostParse();
				Mods.Add(meta);
				Logger.Log("Will load mod without metadata from " + dir + ".");
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
				Logger.Log($"Failed reading assembly: {e}");
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

		protected static void CleanupExtractedZips() {
			string[] folders = Directory.GetDirectories(PathMods);
			foreach(var folder in folders)
				if(folder.EndsWith(zipExtractSuffix))
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
	}
}
