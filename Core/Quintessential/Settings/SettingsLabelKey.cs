using System;

namespace Quintessential.Settings;

[AttributeUsage(AttributeTargets.Field)]
public class SettingsLabelKey : Attribute {

	public string Label;

	public SettingsLabelKey(string label) {
		Label = label;
	}
}
