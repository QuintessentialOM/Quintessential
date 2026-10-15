
using System;

namespace Quintessential.Internal;

internal class QuintessentialUI : QuintessentialMod {
    public static QuintessentialUI Instance { get; }
    public override string ModId => "quintessential_ui";
    public override Type SettingsType => typeof(QuintessentialUISettings);

    public override void Load() { }

    public override void LoadContent() { }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
    public override void Unload() { }
}
