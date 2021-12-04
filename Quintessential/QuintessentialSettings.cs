using Quintessential.Settings;

namespace Quintessential {

	public class QuintessentialSettings {

		[SettingsLabel("Switch Campaign Left")]
		public Keybinding SwitchCampaignLeft = new Keybinding() { Key = "K", Control = true };

		[SettingsLabel("Switch Campaign Right")]
		public Keybinding SwitchCampaignRight = new Keybinding() { Key = "L", Control = true };
	}
}
