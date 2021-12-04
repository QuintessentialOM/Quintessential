using System;

namespace Quintessential.Internal;

public class QuintessentialAsMod : QuintessentialMod {

	public override Type SettingsType => typeof(QuintessentialSettings);

	public override void Load() { }

	public override void PostLoad() { }

	public override void Unload() { }
}
