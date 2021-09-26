using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quintessential {

	class ModsScreen : IScreen {

		// ???
		public bool method_1037() {
			return false;
		}

		// on added (true) or removed (false)
		public void method_47(bool param_4687) {
			
		}

		public void method_48() {
			
		}

		// update & render
		public void method_50(float param_4686) {
			Vector2 size = new Vector2(600f, 700f);
			Vector2 pos = (class_115.field_1433/*screen size*/ / 2 - size / 2).Rounded();
			Vector2 bgPos = pos + new Vector2(78f, 88f);
			Vector2 bgSize = size + new Vector2(-152f, -158f);
			// background
			class_135.method_268(class_238.field_1989.field_102.field_810, Color.White, bgPos, Bounds2.WithSize(bgPos, bgSize));
			// frame
			class_135.method_276(class_238.field_1989.field_102.field_817, Color.White, pos, size);
			// label
			class_140.method_317(class_134.method_253("Mods", string.Empty), pos + new Vector2(95f, 700 - 99f), 600 - 214, true, true);
			// close button & close when clicking outside
			if(class_140.method_323(pos, size, new Vector2(496f, 700 - 98f))) {
				GameLogic.field_2434.field_2464 = false;
				GameLogic.field_2434.method_949();
				class_238.field_1991.field_1873.method_28(1f);
			}
			int y = 0;
			class_135.method_290($"Quintessential v{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", pos - new Vector2(-100, -size.Y + 140 + y), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
			y += 50;
			foreach(var mod in QuintessentialLoader.Mods) {
				class_135.method_290(mod.Name + " (" + mod.Version.ToString() + ")", pos - new Vector2(-100, -size.Y + 140 + y), class_238.field_1990.field_2145, class_181.field_1718, (enum_0) 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
				y += 30;
			}
		}
	}
}
