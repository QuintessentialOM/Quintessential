using Quintessential.Settings;

namespace Quintessential;

public class QuintessentialSettings {

	[SettingsLabel("Switch Campaign Left")]
	public Keybinding SwitchCampaignLeft = new() { Key = "K", Control = true };

	[SettingsLabel("Switch Campaign Right")]
	public Keybinding SwitchCampaignRight = new() { Key = "L", Control = true };
}
