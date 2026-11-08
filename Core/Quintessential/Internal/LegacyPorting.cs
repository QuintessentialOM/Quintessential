using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quintessential.Internal;

internal static class LegacyPorting {
    internal static bool UnsafeLegacyExport = false;
    public static Dictionary<string, LegacyCampaignModel> LegacyCampaignModels = [];
    public static Dictionary<string, LegacyJournalModel> LegacyJournalModels = [];
    public static string ExportFolder = Path.Combine("modded", "Export");

    public static void ImportLegacyCampaign(string path) {
        LegacyCampaignModel c = DataSerializer.Deserialize<LegacyCampaignModel>(path);
        QuintessentialCore.Instance.Log.Info($"Campaign \"{c.Title}\" ({c.Name}) has {c.Chapters.Count} chapters.");
        c.Path = Path.GetDirectoryName(path);
        LegacyCampaignModels.Add(path, c);
    }
    public static void ImportLegacyJournal(string path) {
        LegacyJournalModel c = DataSerializer.Deserialize<LegacyJournalModel>(path);
        QuintessentialCore.Instance.Log.Info($"Journal \"{c.Title}\" has {c.Chapters.Count} chapters.");
        foreach (var chapter in new List<JournalChapterModel>(c.Chapters)) {
            if (chapter.Puzzles.Count != 5) {
                QuintessentialCore.Instance.Log.Warn($"Journal chapter \"{chapter.Title}\" in \"{c.Title}\" has {chapter.Puzzles.Count} puzzles instead of 5; skipping chapter.");
                c.Chapters.Remove(chapter);
            }
        }

        if (c.Chapters.Count > 0) {
            c.Path = Path.GetDirectoryName(path);
            LegacyJournalModels.Add(path, c);
        } else
            QuintessentialCore.Instance.Log.Warn($"Journal \"{c.Title}\" has no chapters, skipping.");
    }

    private static Dictionary<string, Campaign> CreateLegacyCampaigns() {
        Dictionary<string, Campaign> campaigns = [];

        Dictionary<string, Texture> iconCache = [];
        foreach (var c in LegacyCampaignModels) {
            var campaign = new Campaign {
                chapters = new CampaignChapter[c.Value.Chapters.Count]
            };

            ((patch_Campaign)(object)campaign).QuintTitle = Translations.WithAllLanguages(c.Value.Title);

            for (int j = 0; j < c.Value.Chapters.Count; j++) {
                ChapterModel chapter = c.Value.Chapters[j];
                campaign.chapters[j] = new CampaignChapter(
                    Translations.WithAllLanguages(chapter.Title),
                    Translations.WithAllLanguages(chapter.Subtitle),
                    Translations.WithAllLanguages(chapter.Place),
                    chapter.Background != null ? AssetLoaderHelper.LoadTexture(chapter.Background) : Campaigns.opusMagnum.chapters[j].background,
                    Campaigns.opusMagnum.chapters[j].lockedIcon,
                    Campaigns.opusMagnum.chapters[j].unlockedIcon,
                    Campaigns.opusMagnum.chapters[j].hoverIcon,
                    Campaigns.opusMagnum.chapters[j].gemIcon,
                    Campaigns.opusMagnum.chapters[j].buttonOffset,
                    ChapterAlignment.Right
                );

                foreach (var entry in chapter.Entries) {
                    UnlockRequirement requirement = string.IsNullOrEmpty(entry.Requires) ? new UnlockReqNothing() : new UnlockReqCompleteCampaignItem(entry.Requires);

                    var lower = entry.Type.ToLowerInvariant();
                    CampaignItem cItem;
                    switch (lower) {
                        case "puzzle":
                            if (!QuintessentialLoader.TryLoadPuzzle(c.Value.Path, entry.Puzzle, Translations.WithAllLanguages(c.Value.Title), out var puzzle))
                                continue;

                            puzzle.puzzleId = entry.ID;
                            // ensure all inputs/outputs have names
                            foreach (PuzzleInputOutput io in puzzle.inputs.Union(puzzle.outputs)) {
                                if (!io.molecule.displayName.HasValue()) {
                                    if (((patch_Molecule)(object)io.molecule).GetDisplayName().HasValue()) {
                                        io.molecule.displayName = ((patch_Molecule)(object)io.molecule).GetDisplayName().GetValue();
                                    } else
                                        io.molecule.displayName = Translations.WithAllLanguages("Molecule");
                                }
                            }

                            cItem = AddEntryToCampaign(campaign, j, entry.ID, Translations.WithAllLanguages(entry.Title), CampaignItemType.Puzzle, MaybeHelper.empty, puzzle, Assets.musicTracks.solving3, Assets.sounds.fanfare_solving3, requirement, entry.NoStoryPanel);
                            break;
                        case "solitaire":
                            cItem = new(entry.ID, Translations.WithAllLanguages("Sigmar's Garden"), CampaignItemType.Solitaire, MaybeHelper.empty, requirement, Assets.musicTracks.solving1, Assets.sounds.fanfare_solving1, campaign);
                            campaign.chapters[j].campaignItems.Add(cItem);
                            break;
                        case "cutscene":
                            cItem = new(entry.ID, Translations.WithAllLanguages(entry.Title), CampaignItemType.Cutscene, MaybeHelper.empty, requirement, Assets.musicTracks.solving1, Assets.sounds.fanfare_solving1, campaign);
                            break;
                        case "document":
                            cItem = new(entry.ID, Translations.WithAllLanguages(entry.Title), CampaignItemType.Letter, MaybeHelper.empty, requirement, Assets.musicTracks.solving1, Assets.sounds.fanfare_solving1, campaign);
                            break;
                        default:
                            QuintessentialCore.Instance.Log.Warn($"Campaign entry in {c.Value.Name} has unknown type {entry.Type}, skipping");
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

            campaigns.Add(c.Key, campaign);
        }
        return campaigns;
    }
    private static Dictionary<string, Journal> CreateLegacyJournals() {
        Dictionary<string, Journal> journals = [];

        foreach (var journal in LegacyJournalModels) {

            List<JournalVolume> Volumes = [.. journal.Value.Chapters.Select(chapter =>
                new JournalVolume {
                    issueName = Translations.WithAllLanguages(chapter.Title),
                    flavorText = Translations.WithAllLanguages(chapter.Description),
                    puzzles = GetJournalPuzzles(chapter, journal.Value)
            })];
            journals.Add(journal.Key, new Journal() {
                volumes = Volumes,
                title = Translations.WithAllLanguages(journal.Value.Title),
                puzzleBackgroundLarge = string.IsNullOrEmpty(journal.Value.PuzzleBackgroundLarge) ? null : AssetLoaderHelper.LoadTexture(journal.Value.PuzzleBackgroundLarge),
                puzzleBackgroundSmall = string.IsNullOrEmpty(journal.Value.PuzzleBackgroundSmall) ? null : AssetLoaderHelper.LoadTexture(journal.Value.PuzzleBackgroundSmall),
            });
        }
        return journals;
    }
    private static Puzzle[] GetJournalPuzzles(JournalChapterModel chapter, LegacyJournalModel journal) {
        List<Puzzle> puzzles = [];
        foreach (var puzzleFileName in chapter.Puzzles) {
            Puzzle p = QuintessentialLoader.TryLoadPuzzle(journal.Path, puzzleFileName, Translations.Translate(journal.Title), out var puzzle) ? puzzle : new Puzzle();
            puzzles.Add(p);
        }
        return [.. puzzles];
    }

    public static void ExportNewFormat() {
        foreach (var campaign in CreateLegacyCampaigns()) {
            ExportCampaingNewFormat(campaign.Key, ExportFolder, campaign.Value);
        }
        foreach (var journal in CreateLegacyJournals()) {
            ExportJournalNewFormat(journal.Key, ExportFolder, journal.Value);
        }
    }
    private static void ExportCampaingNewFormat(string origPath, string exportFolder, Campaign campaign) {
        throw new NotImplementedException("Exporting a campaign to the new format requeres Quintessential Data!");
    }
    private static void ExportJournalNewFormat(string origPath, string exportFolder, Journal journal) {
        throw new NotImplementedException("Exporting a journal to the new format requeres Quintessential Data!");
    }

    internal static CampaignItem AddEntryToCampaign(
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
    ) {
        if (puzzle.HasValue()) {
            puzzle.GetValue().puzzleTip = puzzleTip;
        }
        CampaignItem campaignItem = new(itemId, itemName, itemType, puzzle, requirement, musicTrack, fanfare, campaign);
        campaign.chapters[chapter].campaignItems.Add(campaignItem);
        // no cutscene to see here
        if (noStoryPanel)
            campaignItem.vignette = MaybeHelper.empty;

        return campaignItem;
    }
}
