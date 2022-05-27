namespace Quintessential.Settings;

/// <summary>
/// Settings fields of this type are displayed as a button on the Mods menu.
/// You must additionally annotate these fields with <code>[YamlDotNet.Serialization.YamlIgnore]</code>.
/// </summary>
public delegate void SettingsButton();
