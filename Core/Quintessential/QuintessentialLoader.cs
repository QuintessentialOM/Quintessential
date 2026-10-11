using Quintessential.Internal;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Quintessential;

public class QuintessentialLoader
{
    public static string PathLightning;
    public static string PathUnpackedMods;
    public static string PathModSaves;
    public static string PathScreenshots;

    public static List<QuintessentialMod> CodeMods = [];
    public static QuintessentialMod CodeModById(string modId) => CodeMods.SingleOrDefault(mod => mod.ModId == modId, null);
    public static List<ModMeta> Mods = [];
    public static ModMeta ModById(string modId) => Mods.SingleOrDefault(mod => mod.ModId == modId, null);
    public static bool IsModPresent(string modId) => Mods.Any(mod => mod.ModId == modId);
    public static List<string> ModContentDirectories = [];
    public static List<string> ModPuzzleDirectories = [];

    public static List<Campaign> AllCampaigns = [];
    public static Campaign VanillaCampaign;
    public static List<List<JournalVolume>> AllJournals = [];
    public static List<JournalVolume> VanillaJournal;

    private static readonly List<string> ModCampaignPaths = [];
    private static readonly List<string> ModJournalPaths = [];
    public static void PreInit()
    {
        try
        {
            PathLightning = Path.GetDirectoryName(typeof(GameLogic).Assembly.Location);
            PathUnpackedMods = Path.Combine(PathLightning, "UnpackedMods");
            PathScreenshots = Path.Combine(PathLightning, "Screenshots");
            PathUnpackedMods = Path.Combine(PathLightning, "UnpackedMods");


            Logger.Init();
            Logger.Log("Starting pre-init loading.");

            // Instantiate code mods
            var modTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(QuintessentialMod).IsAssignableFrom(t) && !t.IsAbstract);
            List<QuintessentialMod> collectedCodeMods = [];
            HashSet<string> idCheckSet = [];

            foreach (var type in modTypes) {
                QuintessentialMod mod = (QuintessentialMod)type.GetConstructor([]).Invoke([]);
                SetCodeModInstance(type, mod);
                collectedCodeMods.Add(mod);


                if (idCheckSet.Contains(mod.ModId)) throw new Exception($"Multiple mods cannot share the same id. Found multiple mods with id '{mod.ModId}'");
                idCheckSet.Add(mod.ModId);
            }

            // Load modMeta files
            string pathToModLoaderData = Path.Combine(PathUnpackedMods, "modLoaderData.json");
            OrderedDictionary<string, string> data = DataSerializer.Deserialize<OrderedDictionary<string, string>>(pathToModLoaderData);

            foreach (var item in data) {
                string metaPath = Path.Combine(PathLightning, item.Value, "modMeta.jsonc");
                ModMeta mod = DataSerializer.Deserialize<ModMeta>(metaPath);
                mod.PathToDirectory = Path.Combine(PathLightning, item.Value);

                var content = Path.Combine(mod.PathToDirectory, "Content");
                if (Directory.Exists(content))
                    ModContentDirectories.Add(mod.PathToDirectory);

                if (!string.IsNullOrWhiteSpace(mod.DLL)) {
                    var codeMod = collectedCodeMods.SingleOrDefault(codeMod => codeMod.ModId == mod.ModId, null);
                    if (codeMod != null) {
                        codeMod.Meta = mod;
                        CodeMods.Add(codeMod);
                    }
                }
                LoadModCampaigns(mod);

                Mods.Add(mod);
            }

            // Add mod content
            foreach (var mod in CodeMods)
                mod.Load();
            Logger.Log($"Finished pre-init loading - {Mods.Count} mods loaded, {CodeMods.Count} of them are code mods; {ModContentDirectories.Count} content directories"/*, and {LegacyCampaignModels.Count} custom campaigns found."*/);
            DataSerializer.WasInit = true;
        }
        catch (Exception e)
        {
            if (Logger.Setup)
            {
                Logger.Log("Failed to pre-initialize!");
                Logger.Log(e);
            }
            throw;
        }
    }
    public static void ModContentInit() {
        Logger.Log("Starting content loading.");
        foreach (var mod in CodeMods)
            mod.LoadContent();

        // This is where DataContentInit ( Recipe & Tag )" gets injected
        foreach (var mod in CodeMods)
            mod.LoadCompatContent();

        Logger.Log("Loading campaigns and journals.");
        LegacyPorting.ExportNewFormat();
        LoadCampaigns();
        LoadJournals();

        Logger.Log("Finalising content.");
        foreach (var mod in CodeMods)
            mod.FinaliseContent();

        Logger.Log("Finished content loading.");
    }
    public static void PostInit()
    {
        Logger.Log("Starting post-init loading.");
        // Read mod save data
        PathModSaves = Path.Combine(OSInfo.GetSavePath(), "ModSettings");
        Logger.Log($"Mod settings directory: \"{PathModSaves}\"");
        if (!Directory.Exists(PathModSaves))
            Directory.CreateDirectory(PathModSaves);
        foreach (var mod in CodeMods)
        {
            var savePath = Path.Combine(PathModSaves, mod.ModId + ".json");
            if (File.Exists(savePath))
            {
                var settings = DataSerializer.Deserialize(savePath, mod.SettingsType);
                if (settings != null)
                {
                    mod.Settings = settings;
                    mod.ApplySettings();
                } else
                    Logger.Log("Loaded null settings for mod " + Translations.Translate(mod.Meta.ModId));
            }
            mod.Settings ??= mod.SettingsType.GetConstructor([]).Invoke([]);
        }
        foreach (var mod in CodeMods)
            mod.PostLoad();
        Logger.Log("Finished post-init loading.");
    }


    private static void LoadModCampaigns(ModMeta mod) {
        var puzzles = Path.Combine(mod.PathToDirectory, "Puzzles");
        if (Directory.Exists(puzzles)) {
            if (!ModPuzzleDirectories.Contains(puzzles))
                ModPuzzleDirectories.Add(puzzles);
            // Look for name.campaign.json and name.journal.jsonc files in the folder
            foreach (var item in Directory.GetFiles(puzzles)) {
                string filename = Path.GetFileName(item);
                if (filename.EndsWith(".campaign.yaml")) {
                    LegacyPorting.ImportLegacyCampaign(item);
                }
                if (filename.EndsWith(".journal.yaml")) {
                    LegacyPorting.ImportLegacyJournal(item);
                }
                // TODO legacy puzzles
                
                if (filename.EndsWith(".campaign.jsonc")) {
                    ModCampaignPaths.Add(item);
                }
                if (filename.EndsWith(".journal.jsonc")) {
                    ModJournalPaths.Add(item);
                }

                // TODO non legacy Jsonc version!
            }
        }
    }
    public static void LoadCampaigns() {
        AllCampaigns.Clear();

        VanillaCampaign = Campaigns.opusMagnum;
        ((patch_Campaign)(object)VanillaCampaign).QuintTitle = Translations.Translate("om");
        ((patch_Campaign)(object)Campaigns.deReMetallica).QuintTitle = Translations.Translate("De Re Metallica");
        AllCampaigns.Add(VanillaCampaign);

        foreach (var path in ModCampaignPaths) {
            var campaign = DataLoader.LoadCampaign(path);

            foreach (var chapter in campaign.chapters) {
                foreach (var item in chapter.campaignItems) {
                    if (item.puzzle.HasValue()) {
                        Array.Resize(ref Puzzles.campaignPuzzles, Puzzles.campaignPuzzles.Length + 1);
                        Puzzles.campaignPuzzles[^1] = item.puzzle.GetValue();
                    }
                }
            }
            AllCampaigns.Add(campaign);
        }
    }
    public static void LoadJournals()
    {
        AllJournals.Clear();

        VanillaJournal = JournalVolumes.volumes.ToList();
        AllJournals.Add(VanillaJournal);

        foreach (string path in ModJournalPaths) {
            var journal = DataLoader.LoadJournal(path);

            // add journal puzzles to list of puzzles
            foreach (JournalVolume volume in journal)
            {
                int l = Puzzles.campaignPuzzles.Length;
                Array.Resize(ref Puzzles.campaignPuzzles, l + volume.puzzles.Length);
                int i = 0;
                foreach (var puzzle in volume.puzzles)
                {
                    Puzzles.campaignPuzzles[l + i] = puzzle;
                    i++;
                } // this is a little bit of a patchy method of doing it, i'm not sure the exact form the journal puzzle array takes and i'm a little bit tired from testing it, also taken from icwass RMC journal code
            }
            // log journals
            foreach (JournalVolume jv in journal)
            {
                Logger.Log($"Journal {jv.issueName} has {jv.puzzles.Length} puzzles");
            }
            AllJournals.Add(journal);
        }
    }

    public static bool TryLoadPuzzle(string basePath, string puzzleFileName, string campaignTitle, out Puzzle puzzle) {
        try {
            string baseName = Path.Combine(basePath, puzzleFileName);
            if (File.Exists(baseName + ".puzzle")) {
                //throw new Exception("Attempted to load a vanilla puzzle.");
                puzzle = Puzzle.LoadFromFile(baseName + ".puzzle");
            } else if (File.Exists(baseName + ".puzzle.jsonc")) {
                if (!IsModPresent("quintessential_data")) throw new Exception("The mod 'quintessential_data' is requered for loading json based puzzles.");
                puzzle = Puzzle.LoadFromFile(baseName + ".puzzle.jsonc");
            } else if (File.Exists(baseName + ".puzzle.json")) {
                if (!IsModPresent("quintessential_data")) throw new Exception("The mod 'quintessential_data' is requered for loading json based puzzles.");
                puzzle = Puzzle.LoadFromFile(baseName + ".puzzle.json");
            } else if (File.Exists(baseName + ".puzzle.yaml")) {
                puzzle = PuzzleModel.FromModel(DataSerializer.Deserialize<PuzzleModel>(baseName + ".puzzle.yaml"));
            } else {
                Logger.Log($"Puzzle \"{puzzleFileName}\" from \"{campaignTitle}\" doesn't exist, ignoring");
                puzzle = null;
                return false;
            }

            // even if it was loaded from a vanilla format puzzle file, it was included in a mod and may rely on modded behaviour
            // these are never saved over and could have been modified directly by the campaign mod, so this is safe
            //((patch_Puzzle)(object)puzzle).IsModdedPuzzle = true;

            return true;
        } catch (Exception e) {
            Logger.Log($"Exception loading puzzle \"{puzzleFileName}\" from \"{campaignTitle}\", ignoring");
            Logger.Log(e);
            puzzle = null;
            return false;
        }
    }
    public static void CheckCampaignReload() {
        if (QuintessentialCoreSettings.Instance.HotReloadCampaigns.Pressed() && GameLogic.instance.GetCurrentScreen() is PuzzleSelectScreen)
        {
            Logger.Log("Reloading campaigns and journals!");

            ModPuzzleDirectories.Clear();

            Campaigns.opusMagnum = VanillaCampaign;
            Campaigns.campaigns[0] = VanillaCampaign;
            JournalVolumes.volumes = [.. VanillaJournal];
            patch_PuzzleSelectScreen.SetCampaign(0);
            patch_JournalScreen.ResetPosition();

            foreach (ModMeta mod in Mods)
                LoadModCampaigns(mod);

            LoadCampaigns();
            LoadJournals();
            UI.InstantCloseScreen();
            UI.OpenScreen(new PuzzleSelectScreen());
        }
    }


    private static void SetCodeModInstance(Type type, QuintessentialMod instance) {
        var property = type.GetProperty("Instance");
        if (property == null || !property.PropertyType.IsAssignableFrom(type) || property.SetMethod != null) {
            Logger.Log($"Failed to find Instance properity for CodeMod.\n" +
                $"Add the following line to the ModClass:\n" +
                $"\tpublic static {type.Name} Instance {{ get; }}");
            throw new Exception("Failed to set instance for a CodeMod.");
        }

        var backingField = type.GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        // Sadly we can't use backingField.SetValue, because the field is InitOnly
        var method = new DynamicMethod(
            name: "SetBackingField",
            returnType: null,
            parameterTypes: [type],
            restrictedSkipVisibility: true
        );
        var IL = method.GetILGenerator();

        IL.Emit(OpCodes.Ldarg, 0);
        IL.Emit(OpCodes.Stsfld, backingField);
        IL.Emit(OpCodes.Ret);

        method.Invoke(null, [instance]);
    }

}
