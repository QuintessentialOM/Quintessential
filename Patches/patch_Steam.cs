using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quintessential.Internal;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006 // Naming Styles

internal class patch_Steam{

	public static extern void orig_method_2150();
	public static void method_2150(){
		orig_method_2150(); // is this necessary?
		Screenshotter.CheckScreenshot();
	}
}
