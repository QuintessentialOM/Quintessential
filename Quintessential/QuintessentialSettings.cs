using Quintessential.Settings;

using YamlDotNet.Serialization;

namespace Quintessential;

public class QuintessentialSettings {

	public static QuintessentialSettings Instance => QuintessentialLoader.QuintessentialAsMod.Settings as QuintessentialSettings;

	//[SettingsLabel("Take Screenshot")]
	//public Keybinding Screenshot = new("F12");

	[SettingsLabel("Hot Reload Campaigns")]
	public Keybinding HotReloadCampaigns = new("F11");

	[SettingsLabel("Enable Campaign Switcher")]
	public bool EnableCustomCampaigns = true;

	[SettingsLabel("Campaign Switcher Options:")]
	public CampaignSwitcherSettings SwitcherSettings = new();

	public class CampaignSwitcherSettings : SettingsGroup {

		public override bool Enabled => Instance.EnableCustomCampaigns;

		[SettingsLabel("Switch Campaign Left")]
		public Keybinding SwitchCampaignLeft = new() { Key = "K", Control = true };
		
		[SettingsLabel("Switch Campaign Right")]
		public Keybinding SwitchCampaignRight = new() { Key = "L", Control = true };
	}

	[SettingsLabel("Dump Puzzles")]
	[YamlIgnore]
	public SettingsButton DumpPuzzles = QuintessentialLoader.DumpVanillaPuzzles;
}
