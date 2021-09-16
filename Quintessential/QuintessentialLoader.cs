using MonoMod.Utils;
using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quintessential {

	public class QuintessentialLoader {

		public readonly static string VersionString = "0.0.2";

		public readonly static int VersionNumber = 1;

		public static string PathLightning;
		public static string PathMods;

		public static List<QuintessentialMod> CodeMods = new List<QuintessentialMod>();
		public static List<ModMeta> Mods = new List<ModMeta>();
		public static List<string> ModContentDirectories = new List<string>();
		private static List<Assembly> Assemblies = new List<Assembly>();

		public static void PreInit() {
			PathLightning = Path.GetDirectoryName(typeof(GameLogic).Assembly.Location);
			PathMods = Path.Combine(PathLightning, "Mods");

			Logger.Init();
			Logger.Log("Starting pre-init loading.");

			QApi.Init();

			if(!Directory.Exists(PathMods))
				Directory.CreateDirectory(PathMods);

			// Find mods in Mods/
			string[] files = Directory.GetFiles(PathMods);
			foreach(var file in files)
				if(file.EndsWith(".zip"))
					LoadZipMod(file);

			string[] folders = Directory.GetDirectories(PathMods);
			foreach(var folder in folders)
				LoadFolderMod(folder);

			// Load dlls
			foreach(var mod in Mods)
				if(!string.IsNullOrWhiteSpace(mod.DLL)) {
					string dllPath = mod.DLL;
					if(!string.IsNullOrEmpty(mod.PathArchive)) {
						// unzip the mod first
					}
					LoadModAssembly(mod, GetRemappedAssembly(dllPath, mod));
				}
			
			// Add mod content
			// Load mods
			foreach(var mod in CodeMods)
				mod.Load();
			Logger.Log($"Finished pre-init loading - {Mods.Count} mods loaded, {CodeMods.Count} assemblies loaded, {ModContentDirectories.Count} content directories found.");
		}

		public static void PostLoad() {
			Logger.Log("Starting post-init loading.");
			foreach(var mod in CodeMods)
				mod.PostLoad();
			Logger.Log("Finished post-init loading.");
		}

		protected static void LoadZipMod(string zip) {

		}

		protected static void LoadFolderMod(string dir) {
			Logger.Log("Loading folder mod: " + dir);
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
							meta.PathDirectory = dir;
							meta.PostParse();
							Mods.Add(meta);
							Logger.Log($"Will load mod \"{meta.Name}\", version {meta.VersionString}.");
						}
					} catch(Exception e) {
						Logger.Log($"Failed parsing quintessential.yaml in {dir}: {e}");
					}
				}
			}

			// Get mod content
			//  - Consider modded folders when fetching any content
			//  - Custom language files: vanilla stores in a big CSV, but for custom dialogue (and languages) we'll want seperate files (e.g. English.txt, French.txt)
			//  - Custom solitaires too?
			var content = Path.Combine(dir, "Content");
			if(Directory.Exists(content))
				ModContentDirectories.Add(dir);
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
