using System;
using System.Collections.Generic;

namespace Quintessential.Internal;

internal static class DataLoader {
    static Dictionary<string, Texture> IconCache = [];
    public static Texture GetIcon(string path) {
        if (IconCache.Count == 0) {
            static void AddTexture(Texture texture) {
                IconCache.Add(texture.sourceFile.GetValue(), texture);
            }
            AddTexture(Assets.textures.puzzle_select.icon_cinematic);
            AddTexture(Assets.textures.puzzle_select.icon_cinematic_small);
            AddTexture(Assets.textures.puzzle_select.icon_document);
            AddTexture(Assets.textures.puzzle_select.icon_document_small);
            AddTexture(Assets.textures.puzzle_select.icon_puzzle);
            AddTexture(Assets.textures.puzzle_select.icon_puzzle_small);
            AddTexture(Assets.textures.puzzle_select.icon_puzzle_drm);
            AddTexture(Assets.textures.puzzle_select.icon_puzzle_small_drm);
            AddTexture(Assets.textures.puzzle_select.icon_solitaire);
            AddTexture(Assets.textures.puzzle_select.icon_solitaire_small);
            AddTexture(Assets.textures.puzzle_select.icon_solitaire_drm);
            AddTexture(Assets.textures.puzzle_select.icon_solitaire_small_drm);
            AddTexture(Assets.textures.puzzle_select.icon_tutorial);
            AddTexture(Assets.textures.puzzle_select.icon_tutorial_small);
            AddTexture(Assets.textures.puzzle_select.icon_tutorial_drm);
            AddTexture(Assets.textures.puzzle_select.icon_tutorial_small_drm);
        }
        if (!IconCache.TryGetValue(path, out var icon)) {
            icon = AssetLoaderHelper.LoadTexture(path);
            IconCache[path] = icon;
        }
        return icon;
    }

    static Dictionary<string, Texture> TextureCache = [];
    public static Texture GetOther(string path) {
        if (TextureCache.Count == 0) {
            static void AddTexture(Texture texture) {
                TextureCache.Add(texture.sourceFile.GetValue(), texture);
            }
            foreach (var chapter in Campaigns.opusMagnum.chapters) {
                AddTexture(chapter.background);
                AddTexture(chapter.unlockedIcon);
                AddTexture(chapter.lockedIcon);
                AddTexture(chapter.hoverIcon);
                AddTexture(chapter.gemIcon);
            }
            AddTexture(Assets.textures.cinematic.backgrounds.greathall_a);
            AddTexture(Assets.textures.cinematic.backgrounds.greathall_b);
            AddTexture(Assets.textures.cinematic.backgrounds.greathall_c);
            AddTexture(Assets.textures.cinematic.backgrounds.tailor_a);
            AddTexture(Assets.textures.cinematic.backgrounds.tailor_b);
            AddTexture(Assets.textures.cinematic.backgrounds.tailor_c);
            AddTexture(Assets.textures.cinematic.backgrounds.dlc_basement_a);
            AddTexture(Assets.textures.cinematic.backgrounds.dlc_basement_b);
            AddTexture(Assets.textures.cinematic.backgrounds.dlc_workshop_a);
            AddTexture(Assets.textures.cinematic.backgrounds.dlc_workshop_b);
            AddTexture(Assets.textures.cinematic.backgrounds.workshop);
        }
        if (!TextureCache.TryGetValue(path, out var texture)) {
            texture = AssetLoaderHelper.LoadTexture(path);
            TextureCache[path] = texture;
        }
        return texture;
    }

    public static void ClearCaches() {
        IconCache = null;
        TextureCache = null;
    }

    public static Campaign LoadCampaign(string path) {
        throw new NotImplementedException("Exporting a campaign to the new format requeres Quintessential Data!");
    }

    public static List<JournalVolume> LoadJournal(string path) {
        throw new NotImplementedException("Exporting a journal to the new format requeres Quintessential Data!");
    }
}
