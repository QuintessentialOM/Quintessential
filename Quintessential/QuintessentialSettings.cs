namespace Quintessential {

	public class QuintessentialSettings {

		[SettingsLabel("Test A")]
		public bool TestA;

		[YamlDotNet.Serialization.YamlIgnore]
		public SettingsButton Log = () => Logger.Log("pressed button");

		[SettingsLabel("Test B")]
		public bool TestB;

		[SettingsLabel("Test C")]
		public bool TestC;

		internal void Apply() {
			
		}
	}
}
