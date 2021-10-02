using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quintessential {

	[AttributeUsage(AttributeTargets.Field)]
	public class SettingsLabel : Attribute {

		public string Label;

		public SettingsLabel(string label) {
			Label = label;
		}
	}
}
