
using MonoMod;

internal class patch_CampaignChapter : CampaignChapter {
    [MonoModIgnore]
    public patch_CampaignChapter(LocString title, LocString name, LocString locationName, Texture background, Texture lockedIcon, Texture unlockedIcon, Texture hoverIcon, Texture gemIcon, Vector2 buttonOffset, ChapterAlignment alignment) : base(title, name, locationName, background, lockedIcon, unlockedIcon, hoverIcon, gemIcon, buttonOffset, alignment) {}
    
    public UnlockRequirement requirement;
}
