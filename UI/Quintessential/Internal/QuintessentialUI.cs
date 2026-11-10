
using System;

namespace Quintessential.Internal;

internal class QuintessentialUI : QuintessentialMod {
    public static QuintessentialUI Instance { get; }
    public override string ModId => "quintessential_ui";
    public override Type SettingsType => typeof(QuintessentialUISettings);

    public override void Load() { }

    public override void LoadContent() {
        patch_MoleculeEditorScreen.CopySound = AssetLoaderHelper.LoadSound("sounds/quintessential/copy");
        patch_MoleculeEditorScreen.PasteSuccess = AssetLoaderHelper.LoadSound("sounds/quintessential/paste_success");
        patch_MoleculeEditorScreen.PasteFailed = AssetLoaderHelper.LoadSound("sounds/quintessential/paste_failed");
        patch_MoleculeEditorScreen.PasteStarts = AssetLoaderHelper.LoadSound("sounds/quintessential/paste_starts");
        Volume.SetVolume(patch_MoleculeEditorScreen.CopySound, 0.1f);
        Volume.SetVolume(patch_MoleculeEditorScreen.PasteSuccess, 0.1f);
        Volume.SetVolume(patch_MoleculeEditorScreen.PasteFailed, 0.1f);
        Volume.SetVolume(patch_MoleculeEditorScreen.PasteStarts, 0.1f);
    }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
    public override void Unload() { }
}
