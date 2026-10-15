using Quintessential.Internal;
using Quintessential.Settings;
using YamlDotNet.Serialization;

namespace Quintessential;

public class QuintessentialUISettings {

    public static QuintessentialUISettings Instance => QuintessentialUI.Instance.Settings as QuintessentialUISettings;

    //[SettingsLabelKey("Take Screenshot")]
    //public Keybinding Screenshot = new("F12");

    [SettingsLabelKey("quintessential_ui.settings.dump_puzzles")]
    [YamlIgnore]
    public SettingsButton DumpPuzzles = Dumping.DumpVanillaPuzzles;

    [SettingsLabelKey("quintessential_ui.settings.dump_atom_sprites")]
    [YamlIgnore]
    public SettingsButton DumpAtomSprites = Dumping.DumpAtomSprites;
}
