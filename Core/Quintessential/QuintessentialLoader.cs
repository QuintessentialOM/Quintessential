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

    public static List<CampaignModel> ModCampaignModels = [];
    public static List<JournalModel> ModJournalModels = [];

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
            Logger.Log($"Finished pre-init loading - {Mods.Count} mods loaded, {CodeMods.Count} of them are code mods; {ModContentDirectories.Count} content directories, and {ModCampaignModels.Count} custom campaigns found.");
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


    private static void LoadModCampaigns(ModMeta mod)
    {
        var puzzles = Path.Combine(mod.PathToDirectory, "Puzzles");
        if (Directory.Exists(puzzles))
        {
            if (!ModPuzzleDirectories.Contains(puzzles))
                ModPuzzleDirectories.Add(puzzles);
            // Look for name.campaign.json and name.journal.json files in the folder
            foreach (var item in Directory.GetFiles(puzzles))
            {
                string filename = Path.GetFileName(item);
                if (filename.EndsWith(".campaign.yaml"))
                {
                    CampaignModel c = DataSerializer.Deserialize<CampaignModel>(item);
                    Logger.Log($"Campaign \"{Translations.Translate(c.TitleKey)}\" ({c.Name}) has {c.Chapters.Count} chapters.");
                    c.Path = Path.GetDirectoryName(item);
                    ModCampaignModels.Add(c);
                }

                if (filename.EndsWith(".journal.yaml"))
                {
                    JournalModel c = DataSerializer.Deserialize<JournalModel>(item);
                    Logger.Log($"Journal \"{Translations.Translate(c.TitleKey)}\" has {c.Chapters.Count} chapters.");
                    foreach (var chapter in new List<JournalChapterModel>(c.Chapters))
                    {
                        if (chapter.Puzzles.Count != 5)
                        {
                            Logger.Log($"Journal chapter \"{Translations.Translate(chapter.TitleKey)}\" in \"{Translations.Translate(c.TitleKey)}\" has {chapter.Puzzles.Count} puzzles instead of 5; skipping chapter.");
                            c.Chapters.Remove(chapter);
                        }
                    }

                    if (c.Chapters.Count > 0)
                    {
                        c.Path = Path.GetDirectoryName(item);
                        ModJournalModels.Add(c);
                    }
                    else
                        Logger.Log($"Journal \"{Translations.Translate(c.TitleKey)}\" has no chapters, skipping.");
                }
            }
        }
    }
    public static void LoadCampaigns()
    {
        AllCampaigns.Clear();

        VanillaCampaign = Campaigns.opusMagnum;
        ((patch_Campaign)(object)VanillaCampaign).QuintTitle = Translations.Translate("om");
        AllCampaigns.Add(VanillaCampaign);

        Dictionary<string, Texture> iconCache = [];

        foreach (var c in ModCampaignModels)
        {
            var campaign = new Campaign
            {
                chapters = new CampaignChapter[c.Chapters.Count]
            };

            ((patch_Campaign)(object)campaign).QuintTitle = Translations.Translate(c.TitleKey);

            for (int j = 0; j < c.Chapters.Count; j++)
            {
                ChapterModel chapter = c.Chapters[j];
                campaign.chapters[j] = new CampaignChapter(
                    Translations.Translate(chapter.TitleKey),
                    Translations.Translate(chapter.SubtitleKey),
                    Translations.Translate(chapter.PlaceKey),
                    chapter.Background != null ? AssetLoaderHelper.LoadTexture(chapter.Background) : Campaigns.opusMagnum.chapters[j].background,
                    Campaigns.opusMagnum.chapters[j].lockedIcon,
                    Campaigns.opusMagnum.chapters[j].unlockedIcon,
                    Campaigns.opusMagnum.chapters[j].hoverIcon,
                    Campaigns.opusMagnum.chapters[j].gemIcon,
                    Campaigns.opusMagnum.chapters[j].buttonOffset,
                    (ChapterAlignment)1
                );

                foreach (var entry in chapter.Entries)
                {
                    UnlockRequirement requirement = string.IsNullOrEmpty(entry.Requires) ? new UnlockReqNothing() : new UnlockReqCompleteCampaignItem(entry.Requires);

                    var lower = entry.Type.ToLowerInvariant();
                    CampaignItem cItem;
                    switch (lower)
                    {
                        case "puzzle":
                                if (!TryLoadPuzzle(c.Path, entry.Puzzle, Translations.Translate(c.TitleKey), out var puzzle))
                                    continue;

                                puzzle.puzzleId = entry.ID;
                                // ensure all inputs/outputs have names
                                foreach (PuzzleInputOutput io in puzzle.inputs.Union(puzzle.outputs))
                                {
                                    if (!io.molecule.displayName.HasValue())
                                    {
                                        io.molecule.displayName = Translations.Translate("Molecule");
                                    }
                                }

                                // TODO: optimize
                                cItem = AddEntryToCampaign(campaign, j, entry.ID, Translations.Translate(entry.TitleKey), CampaignItemType.Puzzle, MaybeHelper.empty, puzzle, Assets.musicTracks.solving3Music, Assets.sounds.fanfare_solving3, requirement, entry.NoStoryPanel);
                                Array.Resize(ref Puzzles.campaignPuzzles, Puzzles.campaignPuzzles.Length + 1);
                                Puzzles.campaignPuzzles[^1] = puzzle;
                                break;
                        case "solitaire":
                                cItem = new(entry.ID, Translations.Translate("Sigmar's Garden"), CampaignItemType.Solitaire, MaybeHelper.empty, requirement, Assets.musicTracks.solving1Music, Assets.sounds.fanfare_solving1, campaign);
                                campaign.chapters[j].campaignItems.Add(cItem);
                                break;
                        case "cutscene":
                                cItem = new(entry.ID, Translations.Translate(entry.TitleKey), CampaignItemType.Cutscene, MaybeHelper.empty, requirement, Assets.musicTracks.solving1Music, Assets.sounds.fanfare_solving1, campaign);
                                break;
                        case "document":
                                cItem = new(entry.ID, Translations.Translate(entry.TitleKey), CampaignItemType.Letter, MaybeHelper.empty, requirement, Assets.musicTracks.solving1Music, Assets.sounds.fanfare_solving1, campaign);
                                break;
                        default:
                            Logger.Log($"Campaign entry in {c.Name} has unknown type {entry.Type}, skipping");
                            continue;
                    }

                    patch_CampaignItem conv = (patch_CampaignItem)(object)cItem;

                    Texture GetIcon(string path) {
                        if (!iconCache.TryGetValue(path, out var icon)) {
                            icon = AssetLoaderHelper.LoadTexture(path);
                            iconCache[path] = icon;
                        }
                        return icon;
                    }

                    if (!string.IsNullOrWhiteSpace(entry.Icon))
                        conv.Icon = GetIcon(entry.Icon);
                    if (!string.IsNullOrWhiteSpace(entry.IconSmall))
                        conv.IconSmall = GetIcon(entry.IconSmall);
                }
            }

            for (int index = 0; index < campaign.chapters.Length; ++index)
                campaign.chapters[index].chapterNumber = index;

            AllCampaigns.Add(campaign);
        }
    }
    public static void LoadJournals()
    {
        AllJournals.Clear();

        VanillaJournal = JournalVolumes.volumes.ToList();
        AllJournals.Add(VanillaJournal);

        foreach (JournalModel journal in ModJournalModels)
        {
            // TODO: allow custom journal images?

            List<JournalVolume> volumes = journal.Chapters.Select(chapter =>
                new JournalVolume
                {
                    issueName = Translations.Translate(chapter.TitleKey),
                    flavorText = Translations.Translate(chapter.DescriptionKey),
                    puzzles = GetJournalPuzzles(chapter,journal)
                }).ToList();

            // add journal puzzles to list of puzzles
            foreach (JournalVolume volume in volumes)
            {
                int l = Puzzles.campaignPuzzles.Length;
                Array.Resize(ref Puzzles.campaignPuzzles, l + volume.puzzles.Length /* should always be 5, but better safe than sorry. */  );
                int i = 0;
                foreach (var puzzle in volume.puzzles)
                {
                    Puzzles.campaignPuzzles[l + i] = puzzle;
                    i++;
                } // this is a little bit of a patchy method of doing it, i'm not sure the exact form the journal puzzle array takes and i'm a little bit tired from testing it, also taken from icwass RMC journal code
            }
            // log journals
            foreach (JournalVolume jv in volumes)
            {
                Logger.Log($"Journal {jv.issueName} has {jv.puzzles.Length} puzzles");
            }
            AllJournals.Add(volumes);
        }
    }
    private static CampaignItem AddEntryToCampaign(
            Campaign campaign,
            int chapter,
            string itemId,
            LocString itemName,
            CampaignItemType itemType,
            Maybe<Tip> puzzleTip,
            Maybe<Puzzle> puzzle,
            MusicTrack musicTrack,
            Sound fanfare,
            UnlockRequirement requirement,
            bool noStoryPanel
    )
    {
        if (puzzle.HasValue())
        {
            //puzzle.method_1087().field_2767 = entryTitle;
            puzzle.GetValue().puzzleTip = puzzleTip;
        }
        CampaignItem campaignItem = new(itemId, itemName, itemType, puzzle, requirement, musicTrack, fanfare, campaign);
        campaign.chapters[chapter].campaignItems.Add(campaignItem);
        // no cutscene to see here
        if (noStoryPanel)
            campaignItem.vignette = MaybeHelper.empty;

        return campaignItem;
    }
    private static Puzzle[] GetJournalPuzzles(JournalChapterModel chapter, JournalModel journal) {
        List<Puzzle> puzzles = [];
        foreach( var puzzleFileName in chapter.Puzzles) {
            Puzzle p = TryLoadPuzzle(journal.Path, puzzleFileName, Translations.Translate(journal.TitleKey), out var puzzle) ? puzzle : new Puzzle();
            puzzles.Add(p);
        }
        return [.. puzzles];
    }
    private static bool TryLoadPuzzle(string basePath, string puzzleFileName, string campaignTitle, out Puzzle puzzle)
    {
        try
        {
            string baseName = Path.Combine(basePath, puzzleFileName);
            if (File.Exists(baseName + ".puzzle")) {
                puzzle = Puzzle.LoadFromFile(baseName + ".puzzle");
            } else if (File.Exists(baseName + ".puzzle.jsonc")) {
                puzzle = PuzzleModel.FromModel(DataSerializer.Deserialize<PuzzleModel>(baseName + ".puzzle.jsonc"));
            } else if (File.Exists(baseName + ".puzzle.json")) {
                puzzle = PuzzleModel.FromModel(DataSerializer.Deserialize<PuzzleModel>(baseName + ".puzzle.json"));
            } else if (File.Exists(baseName + ".puzzle.yaml")) {
                puzzle = PuzzleModel.FromModel(DataSerializer.Deserialize<PuzzleModel>(baseName + ".puzzle.yaml"));
            } else {
                Logger.Log($"Puzzle \"{puzzleFileName}\" from \"{campaignTitle}\" doesn't exist, ignoring");
                puzzle = null;
                return false;
            }

            // even if it was loaded from a vanilla format puzzle file, it was included in a mod and may rely on modded behaviour
            // these are never saved over and could have been modified directly by the campaign mod, so this is safe
            ((patch_Puzzle)(object)puzzle).IsModdedPuzzle = true;

            return true;
        }
        catch (Exception e)
        {
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
            ModCampaignModels.Clear();
            ModJournalModels.Clear();

            Campaigns.opusMagnum = VanillaCampaign;
            Campaigns.campaigns[0] = VanillaCampaign;
            JournalVolumes.volumes = VanillaJournal.ToArray();
            patch_PuzzleSelectScreen.ResetPosition();
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
