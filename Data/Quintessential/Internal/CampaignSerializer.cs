using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintessential.Internal;
internal class CampaignSerializer {
    internal static string CurrentPuzzleFolder = "";
    internal static readonly Codec<UnlockRequirement> UNLOCK_REQ = Codec<UnlockRequirement>.Create(
        Codecs.STRING.Seal("Type", (UnlockRequirement req) => {
            if (req is UnlockReqCompleteCampaignItem) return "Single";
            if (req is UnlockReqCompleteMany) return "Many";
            if (req is UnlockReqCompleteSomeOfChapter) return "SomeOfChapter";
            if (req is UnlockReqCompleteSomeOfMany) return "SomeOfMany";
            if (req is UnlockReqCustom) throw new Exception("Cannot serialze Custom Unlock Requirements.");
            if (req is UnlockReqNever) return "Never";
            if (req is UnlockReqNothing) return "Nothing";
            if (req is UnlockReqPuzzlesComplete) return "PuzzlesComplete";
            throw new Exception("Invalid unlock requerement type found: " + req.GetType());
        }).WithDefault("Nothing"),
        Codecs.STRING.Seal("Id", (UnlockRequirement req) => {
            if (req is UnlockReqCompleteCampaignItem campaignItem) return campaignItem.requiredItemId;
            else return "";
        }).WithDefault(""),
        Codecs.LIST_STRING.Seal("Ids", (UnlockRequirement req) => {
            if (req is UnlockReqCompleteMany many) return [.. many.requiredItemIds];
            else if (req is UnlockReqCompleteSomeOfMany someOfMany) return [.. someOfMany.requiredItemIds];
            else return [];
        }).WithDefault([]),
        Codecs.INT.Seal("Amount", (UnlockRequirement req) => {
            if (req is UnlockReqCompleteSomeOfChapter someOfChapter) return someOfChapter.required;
            else if (req is UnlockReqCompleteSomeOfMany someOfMany) return someOfMany.required;
            else if (req is UnlockReqPuzzlesComplete puzzlesComplete) return puzzlesComplete.requiredMinimum;
            else return 0;
        }).WithDefault(0),
        (type, itemId, itemIds, amount) => {
            return type switch {
                "Single" => new UnlockReqCompleteCampaignItem(itemId),
                "Many" => new UnlockReqCompleteMany([.. itemIds]),
                "SomeOfChapter" => new UnlockReqCompleteSomeOfChapter(amount),
                "SomeOfMany" => new UnlockReqCompleteSomeOfMany([.. itemIds], amount),
                "Never" => new UnlockReqNever(),
                "Nothing" => new UnlockReqNothing(),
                "PuzzlesComplete" => new UnlockReqPuzzlesComplete(amount),
                _ => throw new Exception("Invalid type for Unlock Requirement: " + type + "\nOnly the following types are valid:\n CampaignItem, Many, SomeOfChapter, SomeOfMany, Never, Nothing, PuzzlesComplete"),
            };
        }
    );
    private static readonly Codec<CampaignItemType> ITEM_TYPE = EnumCodec<CampaignItemType>.Create(true, true);
    internal static readonly Codec<CampaignItem> CAMPAIGN_ITEM = Codec<CampaignItem>.Create(
        Codecs.STRING.Seal("Id", (CampaignItem item) => item.itemId),
        Codecs.LOCSTRING.Seal("Name", (CampaignItem item) => item.itemName),
        ITEM_TYPE.Seal("Type", (CampaignItem item) => item.itemType),
        Codecs.STRING.Seal("Puzzle", (CampaignItem item) => item.puzzle.HasValue() ? item.puzzle.GetValue().puzzleId : "").WithDefault(""),
        UNLOCK_REQ.Seal("UnlockRequirement", (CampaignItem item) => item.requirement is UnlockReqNothing ? null : item.requirement).WithDefault(null),
        Codecs.BOOL.Seal("Vignette", (CampaignItem item) => item.vignette.HasValue()).WithDefault(false),
        Codecs.STRING.Seal("Music", (CampaignItem item) => item.musicTrack.field_1739.sourceFileName).WithDefault(""),
        Codecs.STRING.Seal("Fanfare", (CampaignItem item) => item.fanfare.field_4061.sourceFileName).WithDefault(""),
        Codecs.STRING.Seal("IconSmall", (CampaignItem item) => patch_DataLoader.GetTexturePath(item.IconSmall, "")).WithDefault(""),
        Codecs.STRING.Seal("Icon", (CampaignItem item) => patch_DataLoader.GetTexturePath(item.Icon, "")).WithDefault(""),
        (id, name, type, puzzleId, unlockReq, vignette, music, fanfare, iconSmall, icon) => {
            Maybe<Puzzle> p = MaybeHelper.empty;
            if (puzzleId != "") {
                if (!QuintessentialLoader.TryLoadPuzzle(CurrentPuzzleFolder, puzzleId, name, out Puzzle pz))
                    if (!Puzzles.campaignPuzzles.Any(puzz => puzz.puzzleId == puzzleId))
                        throw new Exception("Failed to find puzzle: " + puzzleId);
                    else p = Puzzles.campaignPuzzles.First(puzz => puzz.puzzleId == puzzleId);
                else p = pz;
            }
            // TODO handle Music, & Fanfare!
            var item = new CampaignItem(id, name, type, p, unlockReq ?? new UnlockReqNothing(),
                music == "" ? Assets.musicTracks.solving3 : AssetLoaderHelper.LoadMusic(music),
                fanfare == "" ? Assets.sounds.fanfare_solving3 : AssetLoaderHelper.LoadSound(fanfare),
                null
            );
            if (icon != "")
                item.Icon = icon == "" ? null : patch_DataLoader.GetIcon(icon);
            if (iconSmall != "")
                item.IconSmall = iconSmall == "" ? null : patch_DataLoader.GetIcon(iconSmall);
            if (!vignette)
                item.vignette = MaybeHelper.empty;
            return item;
        }
    );
    private static readonly Codec<List<CampaignItem>> LIST_CAMPAIGN_ITEM = ListCodec<CampaignItem>.Create(CAMPAIGN_ITEM);
    private static readonly Codec<ChapterAlignment> CHAPTER_ALIGNMENT = EnumCodec<ChapterAlignment>.Create(true);
    internal static Codec<CampaignChapter> CHAPTER = Codec<CampaignChapter>.Create(
        Codecs.LOCSTRING.Seal("Title", (CampaignChapter chapter) => chapter.title),
        Codecs.LOCSTRING.Seal("Name", (CampaignChapter chapter) => chapter.name),
        Codecs.LOCSTRING.Seal("Location", (CampaignChapter chapter) => chapter.locationName),
        LIST_CAMPAIGN_ITEM.Seal("Items", (CampaignChapter chapter) => chapter.campaignItems),
        Codecs.STRING.Seal("Background", (CampaignChapter chapter) => patch_DataLoader.GetTexturePath(chapter.background, "")).WithDefault(""),
        Codecs.STRING.Seal("LockedIcon", (CampaignChapter chapter) => patch_DataLoader.GetTexturePath(chapter.lockedIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("UnlockedIcon", (CampaignChapter chapter) => patch_DataLoader.GetTexturePath(chapter.unlockedIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("HoverIcon", (CampaignChapter chapter) => patch_DataLoader.GetTexturePath(chapter.hoverIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("GemIcon", (CampaignChapter chapter) => patch_DataLoader.GetTexturePath(chapter.gemIcon, "")).WithDefault(""),
        Codecs.VECTOR2.Seal("ButtonOffset", (CampaignChapter chapter) => chapter.buttonOffset).WithDefault(Vector2.Zero),
        CHAPTER_ALIGNMENT.Seal("Aligment", (CampaignChapter chapter) => chapter.alignment).WithDefault(ChapterAlignment.Right),
        (title, name, location, items, background, lockedIcon, unlockedIcon, hoverIcon, gemIcon, buttonOffset, aligment) => {
            return new CampaignChapter(title, name, location,
                background == "" ? null : patch_DataLoader.GetOther(background),
                lockedIcon == "" ? null : patch_DataLoader.GetOther(lockedIcon),
                unlockedIcon == "" ? null : patch_DataLoader.GetOther(unlockedIcon),
                hoverIcon == "" ? null : patch_DataLoader.GetOther(hoverIcon),
                gemIcon == "" ? null : patch_DataLoader.GetOther(gemIcon),
                buttonOffset, aligment
            ) {
                campaignItems = items
            };
        }
    );
    private static readonly Codec<List<CampaignChapter>> LIST_CHAPTER = ListCodec<CampaignChapter>.Create(CHAPTER);
    private static readonly Codec<SolitaireType> SOLITAIRE_TYPE = EnumCodec<SolitaireType>.Create(true, true);
    internal static Codec<Campaign> CAMPAIGN = Codec<Campaign>.Create(
        Codecs.LOCSTRING.Seal("Title", (Campaign campaign) => campaign.QuintTitle),
        Codecs.BOOL.Seal("SingleScreen", (Campaign campaign) => campaign.sameScreenChapters).WithDefault(false),
        Codecs.STRING.Seal("PuzzleIcon", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.puzzleIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("PuzzleIconSmall", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.puzzleIconSmall, "")).WithDefault(""),
        Codecs.STRING.Seal("TutorialIcon", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.tutorialIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("TutorialIconSmall", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.tutorialIconSmall, "")).WithDefault(""),
        Codecs.STRING.Seal("SolitaireIcon", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.solitaireIcon, "")).WithDefault(""),
        Codecs.STRING.Seal("SolitaireIconSmall", (Campaign campaign) => patch_DataLoader.GetTexturePath(campaign.solitaireIconSmall, "")).WithDefault(""),
        SOLITAIRE_TYPE.Seal("SolitaireType", (Campaign campaign) => campaign.solitaireType).WithDefault(SolitaireType.Normal),
        LIST_CHAPTER.Seal("Chapters", (Campaign campaign) => [.. campaign.chapters]),
        (title, singleScreen, puzzleIcon, puzzleIconSmall, tutorialIcon, tutorialIconSmall, solitaireIcon, solitaireIconSmall, solitaireType, chapters) => {
            var c = new Campaign {
                QuintTitle = title,
                sameScreenChapters = singleScreen,
                chapters = [.. chapters],
                chapterSelectBaseHeight = 41f,
                solitaireType = solitaireType,
                puzzleIcon = puzzleIcon == "" ? Campaigns.opusMagnum.puzzleIcon : patch_DataLoader.GetIcon(puzzleIcon),
                puzzleIconSmall = puzzleIconSmall == "" ? Campaigns.opusMagnum.puzzleIconSmall : patch_DataLoader.GetIcon(puzzleIconSmall),
                tutorialIcon = tutorialIcon == "" ? Campaigns.opusMagnum.tutorialIcon : patch_DataLoader.GetIcon(tutorialIcon),
                tutorialIconSmall = tutorialIconSmall == "" ? Campaigns.opusMagnum.tutorialIconSmall : patch_DataLoader.GetIcon(tutorialIconSmall),
                solitaireIcon = solitaireIcon == "" ? Campaigns.opusMagnum.solitaireIcon : patch_DataLoader.GetIcon(solitaireIcon),
                solitaireIconSmall = solitaireIconSmall == "" ? Campaigns.opusMagnum.solitaireIconSmall : patch_DataLoader.GetIcon(solitaireIconSmall)
            };
            for (int i = 0; i < c.chapters.Length; i++) {
                var chapter = c.chapters[i];
                chapter.chapterNumber = i;
                chapter.campaign = c;
                foreach (var item in chapter.campaignItems) {
                    item.campaign = c;
                }
                if (i < Campaigns.opusMagnum.chapters.Length) {
                    if (chapter.buttonOffset.X == 0 && chapter.buttonOffset.Y == 0)
                        chapter.buttonOffset = Campaigns.opusMagnum.chapters[i].buttonOffset;
                    chapter.background ??= Campaigns.opusMagnum.chapters[i].background;
                    chapter.lockedIcon ??= Campaigns.opusMagnum.chapters[i].lockedIcon;
                    chapter.unlockedIcon ??= Campaigns.opusMagnum.chapters[i].unlockedIcon;
                    chapter.gemIcon ??= Campaigns.opusMagnum.chapters[i].gemIcon;
                    chapter.hoverIcon ??= Campaigns.opusMagnum.chapters[i].hoverIcon;
                }
            }
            return c;
        }
    );

    internal static Codec<JournalVolume> VOLUME = Codec<JournalVolume>.Create(
        Codecs.LOCSTRING.Seal("IssueName", (JournalVolume volume) => volume.issueName),
        Codecs.LOCSTRING.Seal("FlavorText", (JournalVolume volume) => volume.flavorText).WithDefault(LocString.emptyString),
        Codecs.LIST_STRING.Seal("Puzzles", (JournalVolume volume) => [.. volume.puzzles.Select(p => p.puzzleId)]),
        (issueName, flavorText, puzzleIds) => {
            List<Puzzle> puzzles = [];
            foreach (var id in puzzleIds) {
                if (!QuintessentialLoader.TryLoadPuzzle(CurrentPuzzleFolder, id, issueName, out Puzzle pz))
                    if (!Puzzles.campaignPuzzles.Any(puzz => puzz.puzzleId == id))
                        throw new Exception("Failed to find puzzle: " + id);
                    else puzzles.Add(Puzzles.campaignPuzzles.First(puzz => puzz.puzzleId == id));
                else puzzles.Add(pz);
            }
            return new JournalVolume() {
                issueName = issueName,
                flavorText = flavorText,
                puzzles = [.. puzzles]
            };
        }
    );
    private static readonly Codec<List<JournalVolume>> LIST_VOLUME = ListCodec<JournalVolume>.Create(VOLUME);
    internal static Codec<Journal> JOURNAL = Codec<Journal>.Create(
        Codecs.LOCSTRING.Seal("Title", (Journal journal) => journal.title),
        Codecs.STRING.Seal("Background", (Journal journal) => patch_DataLoader.GetTexturePath(journal.puzzleBackgroundLarge, "")).WithDefault(""),
        Codecs.STRING.Seal("BackgroundSmall", (Journal journal) => patch_DataLoader.GetTexturePath(journal.puzzleBackgroundSmall, "")).WithDefault(""),
        LIST_VOLUME.Seal("Volumes", (Journal journal) => journal.volumes),
        (title, background, backgroundSmall, volumes) => {
            return new Journal() {
                title = title,
                volumes = volumes,
                puzzleBackgroundSmall = backgroundSmall == "" ? null : patch_DataLoader.GetOther(backgroundSmall),
                puzzleBackgroundLarge = background == "" ? null : patch_DataLoader.GetOther(background)
            };
        }
    );
}
