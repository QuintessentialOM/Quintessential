using Quintessential.Internal;
using Quintessential.Settings;

namespace Quintessential;

public class QuintessentialCoreSettings {

	public static QuintessentialCoreSettings Instance => QuintessentialCore.Instance.Settings as QuintessentialCoreSettings;

	[SettingsLabelKey("quintessential_core.settings.hot_reload")]
	public Keybinding HotReloadCampaigns = new("F11");

	[SettingsLabelKey("quintessential_core.settings.custom_campaigns")]
	public bool EnableCustomCampaigns = true;

	[SettingsLabelKey("quintessential_core.settings.switcher")]
	public CampaignSwitcherSettings SwitcherSettings = new();

	public class CampaignSwitcherSettings : SettingsGroup {

		public override bool Enabled => Instance.EnableCustomCampaigns;

		[SettingsLabelKey("quintessential_core.settings.switcher.left")]
		public Keybinding SwitchCampaignLeft = new() { Key = "K", Control = true };
		
		[SettingsLabelKey("quintessential_core.settings.switcher.right")]
		public Keybinding SwitchCampaignRight = new() { Key = "L", Control = true };
	}
}
