using Quintessential.Settings;
using System.IO;
using System.Linq;

namespace Quintessential {

	class ModsScreen : IScreen {

		private const int modButtonWidth = 300;
		ModMeta selected = QuintessentialLoader.QuintessentialModMeta;

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
			Vector2 size = new Vector2(1000f, 922f);
			Vector2 pos = (Input.ScreenSize() / 2 - size / 2).Rounded();
			Vector2 bgPos = pos + new Vector2(78f, 88f);
			Vector2 bgSize = size + new Vector2(-152f, -158f);
			// background
			class_135.method_268(class_238.field_1989.field_102.field_810, Color.White, bgPos, Bounds2.WithSize(bgPos, bgSize));
			// frame
			class_135.method_276(class_238.field_1989.field_102.field_817, Color.White, pos, size);
			// label
			class_140.method_317(class_134.method_253("Mods", string.Empty), pos + new Vector2(100f, size.Y - 99f), modButtonWidth, true, true);
			// close button & close when clicking outside
			if(class_140.method_323(pos, size, new Vector2(size.X - 104f, size.Y - 98f))) {
				GameLogic.field_2434.field_2464 = false;
				GameLogic.field_2434.method_949();
				class_238.field_1991.field_1873.method_28(1f);
			}
			// draw mod buttons
			int y = 40;
			if(class_140.method_315("Quintessential", $"{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", pos - new Vector2(-100, -size.Y + 140 + y), modButtonWidth, selected == QuintessentialLoader.QuintessentialModMeta).method_824(true, true))
				selected = QuintessentialLoader.QuintessentialModMeta;
			y += 100;
			class_135.method_275(class_238.field_1989.field_102.field_822, Color.White, Bounds2.WithSize(pos - new Vector2(-100, -size.Y + 140 + 60), new Vector2(modButtonWidth, 3f)));
			foreach(var mod in QuintessentialLoader.Mods) {
				if(mod != QuintessentialLoader.QuintessentialModMeta) {
					if(class_140.method_315(mod.Name, mod.Version.ToString(), pos - new Vector2(-100, -size.Y + 140 + y), modButtonWidth, selected == mod).method_824(true, true))
						selected = mod;
					y += 70;
				}
			}
			// draw mod options panel
			class_135.method_272(class_238.field_1989.field_102.field_824, pos + new Vector2(modButtonWidth + 110, 76f));
			DrawModOptions(pos + new Vector2(modButtonWidth + 140, -10), size - new Vector2(160, 10), selected);
		}

		private void DrawModOptions(Vector2 pos, Vector2 size, ModMeta mod) {
			float descHeight = DrawModLabel(mod, pos, size);
			foreach(var cmod in QuintessentialLoader.CodeMods)
				if(cmod.Meta == mod)
					if(DrawModSettings(cmod.Settings, pos - new Vector2(0, descHeight), size)) {
						cmod.ApplySettings();
						SaveSettings(mod, cmod.Settings);
					}
		}

		private float DrawModLabel(ModMeta mod, Vector2 pos, Vector2 bgSize) { 
			class_135.method_290(mod.Title ?? mod.Name, pos + new Vector2(20, bgSize.Y - 99f), class_238.field_1990.field_2146, class_181.field_1718, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
			string ver = mod.Version.ToString();
			if(mod.Title != null)
				ver = mod.Name + " - " + ver;
			class_135.method_290(ver, pos + new Vector2(20, bgSize.Y - 130f), class_238.field_1990.field_2145, Color.LightGray, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
			if(mod.Desc != null) {
				var desc = class_135.method_290(mod.Desc, pos + new Vector2(20, bgSize.Y - 170f), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)0, 1f, 0.6f, 460, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
				return desc.Height + 80;
			}
			return 20;
		}

		private bool DrawModSettings(object settings, Vector2 pos, Vector2 bgSize) {
			float y = 170;
			bool settingsChanged = false;
			if(settings == null)
				return false;
			foreach(var field in settings.GetType().GetFields()) {
				string label = field.GetCustomAttributes(true).TakeWhile(att => att is SettingsLabel).Select(att => ((SettingsLabel)att).Label).FirstOrDefault() ?? field.Name;
				if(field.FieldType == typeof(bool)) {
					if(DrawCheckbox(pos + new Vector2(20, bgSize.Y - y), label, (bool)field.GetValue(settings))) {
						field.SetValue(settings, !(bool)field.GetValue(settings));
						settingsChanged = true;
					}
				} else if(field.FieldType == typeof(SettingsButton)) {
					if(class_140.method_314(label, pos + new Vector2(20, bgSize.Y - y - 15)).method_824(true, true))
						((SettingsButton)field.GetValue(settings))();
					y += 20;
				} else if(field.FieldType == typeof(Keybinding)) {
					Keybinding key = (Keybinding)field.GetValue(settings);
					Bounds2 labelBounds = class_135.method_290(label + ": " + (key.Control ? "Control + " : "") + (key.Alt ? "Alt + " : "") + (key.Shift ? "Shift + " : ""), pos + new Vector2(20, bgSize.Y - y - 15), class_238.field_1990.field_2143, class_181.field_1718, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
					var text = !string.IsNullOrWhiteSpace(key.Key) ? key.Key : "None";
					if(class_140.class_149.method_348(text, labelBounds.BottomRight + new Vector2(10, 0), new Vector2(50, (int)labelBounds.Height)).method_824(true, true))
						GameLogic.field_2434.method_946(new ChangeKeybindScreen(key));
					y += 20;
				}
				y += 40;
			}
			return settingsChanged;
		}

		private bool DrawCheckbox(Vector2 pos, string label, bool enabled) {
			Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
			Bounds2 labelBounds = class_135.method_290(label, pos + new Vector2(45f, 13f), class_238.field_1990.field_2143, class_181.field_1718, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
			if(enabled)
				class_135.method_272(class_238.field_1989.field_101.field_773, boxBounds.Min);
			if(boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos())) {
				class_135.method_272(class_238.field_1989.field_101.field_774, boxBounds.Min);
				if(!Input.IsLeftClickPressed())
					return false;
				class_238.field_1991.field_1821.method_28(1f);
				return true;
			} else
				class_135.method_272(class_238.field_1989.field_101.field_772, boxBounds.Min);
			return false;
		}

		private void SaveSettings(ModMeta mod, object settings) {
			string name = mod.Name;
			string path = Path.Combine(QuintessentialLoader.PathModSaves, name + ".yaml");
			if(!Directory.Exists(QuintessentialLoader.PathModSaves))
				Directory.CreateDirectory(QuintessentialLoader.PathModSaves);
			using(StreamWriter writer = new StreamWriter(path))
				YamlHelper.Serializer.Serialize(writer, settings, QuintessentialLoader.CodeMods.First(c => c.Meta == mod).SettingsType);
		}
	}
}