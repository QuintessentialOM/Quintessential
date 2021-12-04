using System;

namespace Quintessential.Settings;

[AttributeUsage(AttributeTargets.Field)]
public class SettingsLabel : Attribute {

	public string Label;

	public SettingsLabel(string label) {
		Label = label;
	}
}
