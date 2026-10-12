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
    static Dictionary<string, Texture> VialCache = [];
    public static Texture GetVial(string path) {
        if (VialCache.Count == 0) {
            static void AddTexture(Texture texture) {
                VialCache.Add(texture.sourceFile.GetValue(), texture);
            }
            AddTexture(Assets.textures.pipelines.vials.abrasive_powder.empty);
            AddTexture(Assets.textures.pipelines.vials.abrasive_powder.filling);
            AddTexture(Assets.textures.pipelines.vials.aether_detector.full);
            AddTexture(Assets.textures.pipelines.vials.aether_detector.empty);
            AddTexture(Assets.textures.pipelines.vials.aether_detector.draining);
            AddTexture(Assets.textures.pipelines.vials.aether_detector.filling);
            AddTexture(Assets.textures.pipelines.vials.conductive_enamel.empty);
            AddTexture(Assets.textures.pipelines.vials.conductive_enamel.filling);
            AddTexture(Assets.textures.pipelines.vials.dental_amalgam.empty);
            AddTexture(Assets.textures.pipelines.vials.dental_amalgam.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_copper.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_copper.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_earth.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_earth.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_gold.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_gold.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_gold.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_gold.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_iron.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_iron.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_iron.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_iron.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_lead.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_lead.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_lead.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_lead.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_mors.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_mors.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_quicksilver.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_quicksilver.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_quicksilver.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_quicksilver.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_salt.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_salt.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_silver.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_silver.filling);
            AddTexture(Assets.textures.pipelines.vials.elemental_tin.full);
            AddTexture(Assets.textures.pipelines.vials.elemental_tin.empty);
            AddTexture(Assets.textures.pipelines.vials.elemental_tin.draining);
            AddTexture(Assets.textures.pipelines.vials.elemental_tin.filling);
            AddTexture(Assets.textures.pipelines.vials.eyedrops.vitae_empty);
            AddTexture(Assets.textures.pipelines.vials.eyedrops.vitae_filling);
            AddTexture(Assets.textures.pipelines.vials.eyedrops.mors_empty);
            AddTexture(Assets.textures.pipelines.vials.eyedrops.mors_filling);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.forest_empty);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.forest_filling);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.brook_empty);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.brook_filling);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.meadow_empty);
            AddTexture(Assets.textures.pipelines.vials.fragrant_powder.meadow_filling);
            AddTexture(Assets.textures.pipelines.vials.generic.empty);
            AddTexture(Assets.textures.pipelines.vials.hexstabilized_salt.empty);
            AddTexture(Assets.textures.pipelines.vials.hexstabilized_salt.filling);
            AddTexture(Assets.textures.pipelines.vials.hydrated_hexstabilized_salt.full);
            AddTexture(Assets.textures.pipelines.vials.hydrated_hexstabilized_salt.draining);
            AddTexture(Assets.textures.pipelines.vials.lamplight_gas.empty);
            AddTexture(Assets.textures.pipelines.vials.lamplight_gas_precursor.full);
            AddTexture(Assets.textures.pipelines.vials.lustre.empty);
            AddTexture(Assets.textures.pipelines.vials.lustre.filling);
            AddTexture(Assets.textures.pipelines.vials.metallic_cinnabar.full);
            AddTexture(Assets.textures.pipelines.vials.metallic_cinnabar.draining);
            AddTexture(Assets.textures.pipelines.vials.preserving_wax.empty);
            AddTexture(Assets.textures.pipelines.vials.preserving_wax.filling);
            AddTexture(Assets.textures.pipelines.vials.rat_poison.empty);
            AddTexture(Assets.textures.pipelines.vials.rat_poison.filling);
            AddTexture(Assets.textures.pipelines.vials.reactive_earth.full);
            AddTexture(Assets.textures.pipelines.vials.reactive_earth.draining);
            AddTexture(Assets.textures.pipelines.vials.refined_hematite.full);
            AddTexture(Assets.textures.pipelines.vials.refined_hematite.draining);
            AddTexture(Assets.textures.pipelines.vials.rocket_fuel.empty);
            AddTexture(Assets.textures.pipelines.vials.rocket_fuel.filling);
            AddTexture(Assets.textures.pipelines.vials.silver_paint.empty);
            AddTexture(Assets.textures.pipelines.vials.silver_paint.filling);
            AddTexture(Assets.textures.pipelines.vials.solvent.empty);
            AddTexture(Assets.textures.pipelines.vials.solvent.filling);
            AddTexture(Assets.textures.pipelines.vials.special_amaro.full);
            AddTexture(Assets.textures.pipelines.vials.special_amaro.empty);
            AddTexture(Assets.textures.pipelines.vials.special_amaro.draining);
            AddTexture(Assets.textures.pipelines.vials.special_amaro.filling);
            AddTexture(Assets.textures.pipelines.vials.stabilized_air.full);
            AddTexture(Assets.textures.pipelines.vials.stabilized_earth.full);
            AddTexture(Assets.textures.pipelines.vials.stabilized_earth.draining);
            AddTexture(Assets.textures.pipelines.vials.stabilized_fire.full);
            AddTexture(Assets.textures.pipelines.vials.stabilized_fire.draining);
            AddTexture(Assets.textures.pipelines.vials.stabilized_iron.full);
            AddTexture(Assets.textures.pipelines.vials.stabilized_iron.draining);
            AddTexture(Assets.textures.pipelines.vials.stabilized_water.full);
            AddTexture(Assets.textures.pipelines.vials.stabilized_water.draining);
            AddTexture(Assets.textures.pipelines.vials.tinstone.full);
            AddTexture(Assets.textures.pipelines.vials.tinstone.draining);
            AddTexture(Assets.textures.pipelines.vials.vapor_of_levity.empty);
            AddTexture(Assets.textures.pipelines.vials.viscous_sludge.empty);
            AddTexture(Assets.textures.pipelines.vials.viscous_sludge.filling);
            AddTexture(Assets.textures.pipelines.vials.welding_thermite.empty);
            AddTexture(Assets.textures.pipelines.vials.welding_thermite.filling);
        }
        if (!VialCache.TryGetValue(path, out var icon)) {
            icon = AssetLoaderHelper.LoadTexture(path);
            VialCache[path] = icon;
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
        VialCache = null;
        TextureCache = null;
    }

    public static Campaign LoadCampaign(string path) {
        throw new NotImplementedException("Exporting a campaign to the new format requeres Quintessential Data!");
    }

    public static Journal LoadJournal(string path) {
        throw new NotImplementedException("Exporting a journal to the new format requeres Quintessential Data!");
    }
}
