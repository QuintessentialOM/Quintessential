namespace Quintessential {

	/// <summary>
	/// Settings fields of this type are displayed as a button on the Mods menu.
	/// Annotate these fields with <code>[YamlDotNet.Serialization.YamlIgnore]</code>, or the pogram will crash if settings are changed.
	/// </summary>
	public delegate void SettingsButton();
}
