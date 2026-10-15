using System;

namespace Quintessential.Internal;

internal class QuintessentialCore : QuintessentialMod {
    public static QuintessentialCore Instance { get; }
    public override string ModId => "quintessential_core";
    public override Type SettingsType => typeof(QuintessentialCoreSettings);

    public override void Load() {}

    public override void LoadContent() { }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
