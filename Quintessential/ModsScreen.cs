using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Quintessential.Settings;

namespace Quintessential;

class ModsScreen : IScreen {

	private const int modButtonWidth = 300;
	ModMeta selected = QuintessentialLoader.QuintessentialModMeta;

	private struct DrawProgress {
		public bool pressed;
		public float curY;
	}

	public bool method_1037() {
		return false;
	}

	public void method_47(bool param_4687) {

	}

	public void method_48() {

	}

	// update & render
	public void method_50(float param_4686) {
		Vector2 size = new(1000f, 922f);
		Vector2 pos = (Input.ScreenSize() / 2 - size / 2).Rounded();
		Vector2 bgPos = pos + new Vector2(78f, 88f);
		Vector2 bgSize = size + new Vector2(-152f, -158f);

		UI.DrawUiBackground(bgPos, bgSize);
		UI.DrawUiFrame(pos, size);
		UI.DrawHeader("Mods", pos + new Vector2(100f, size.Y - 99f), modButtonWidth, true, true);

		if(UI.DrawAndCheckCloseButton(pos, size, new Vector2(104, 94)))
			UI.HandleCloseButton();

		// draw mod buttons
		int y = 40;
		if(UI.DrawAndCheckSolutionButton("Quintessential", $"{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", pos - new Vector2(-100, -size.Y + 140 + y), modButtonWidth, selected == QuintessentialLoader.QuintessentialModMeta))
			selected = QuintessentialLoader.QuintessentialModMeta;
		y += 100;
		class_135.method_275(class_238.field_1989.field_102.field_822, Color.White, Bounds2.WithSize(pos - new Vector2(-100, -size.Y + 140 + 60), new Vector2(modButtonWidth, 3f)));
		foreach(var mod in QuintessentialLoader.Mods) {
			if(mod != QuintessentialLoader.QuintessentialModMeta) {
				if(UI.DrawAndCheckSolutionButton(mod.Name, mod.Version.ToString(), pos - new Vector2(-100, -size.Y + 140 + y), modButtonWidth, selected == mod))
					selected = mod;
				y += 70;
			}
		}
		// draw mod options panel
		UI.DrawTexture(class_238.field_1989.field_102.field_824, pos + new Vector2(modButtonWidth + 110, 76f));
		DrawModOptions(pos + new Vector2(modButtonWidth + 140, -10), size - new Vector2(160, 10), selected);
	}

	private void DrawModOptions(Vector2 pos, Vector2 size, ModMeta mod) {
		float descHeight = DrawModLabel(mod, pos, size);
		foreach(var cmod in QuintessentialLoader.CodeMods)
			if(cmod.Meta == mod)
				if(DrawModSettings(cmod, pos - new Vector2(0, descHeight), size))
					SaveSettings(cmod);
	}

	private float DrawModLabel(ModMeta mod, Vector2 pos, Vector2 bgSize) {
		UI.DrawText(mod.Title ?? mod.Name, pos + new Vector2(20, bgSize.Y - 99f), UI.Title, class_181.field_1718, TextAlignment.Left);
		string ver = mod.Version.ToString();
		if(mod.Title != null)
			ver = mod.Name + " - " + ver;
		UI.DrawText(ver, pos + new Vector2(20, bgSize.Y - 130f), UI.Text, Color.LightGray, TextAlignment.Left);
		if(mod.Desc != null) {
			var desc = UI.DrawText(mod.Desc, pos + new Vector2(20, bgSize.Y - 170f), UI.Text, class_181.field_1718, TextAlignment.Left, maxWidth: 460);
			return desc.Height + 80;
		}
		return 20;
	}

	private bool DrawModSettings(QuintessentialMod mod, Vector2 pos, Vector2 bgSize) {
		var settings = mod.Settings;
		if(settings == null)
			return false;
		return DrawSettingsObject(mod, settings, pos, bgSize, 170).pressed;
	}

	private DrawProgress DrawSettingsObject(QuintessentialMod mod, object settings, Vector2 pos, Vector2 bgSize, float startY) {
		float y = startY;
		bool settingsChanged = false;
		if(settings == null)
			return new DrawProgress { pressed = false, curY = 0 };
		foreach(var field in settings.GetType().GetFields()) {
			if(field.IsStatic)
				continue;

			string label = FromAttr<SettingsLabel, string>(field, y => y.Label, field.Name);

			if(field.FieldType == typeof(bool)) {
				if(DrawCheckbox(pos + new Vector2(20, bgSize.Y - y), label, (bool)field.GetValue(settings))) {
					field.SetValue(settings, !(bool)field.GetValue(settings));
					settingsChanged = true;
				}
			} else if(field.FieldType == typeof(SettingsButton)) {
				if(UI.DrawAndCheckBoxButton(label, pos + new Vector2(20, bgSize.Y - y - 15)))
					((SettingsButton)field.GetValue(settings))();
				y += 20;
			} else if(field.FieldType == typeof(Keybinding)) {
				Keybinding key = (Keybinding)field.GetValue(settings);
				Bounds2 labelBounds = UI.DrawText(label + ": " + key.ControlKeysText(), pos + new Vector2(20, bgSize.Y - y - 15), UI.SubTitle, class_181.field_1718, TextAlignment.Left);
				var text = !string.IsNullOrWhiteSpace(key.Key) ? key.Key : "None";
				if(UI.DrawAndCheckSimpleButton(text, labelBounds.BottomRight + new Vector2(10, 0), new Vector2(50, (int)labelBounds.Height)))
					UI.OpenScreen(new ChangeKeybindScreen(key, label, mod));
				y += 20;
			} else if(typeof(SettingsGroup).IsAssignableFrom(field.FieldType)) {
				SettingsGroup group = (SettingsGroup)field.GetValue(settings);
				var textPos = pos + new Vector2(20, bgSize.Y - y + 5);
				if(group.Enabled) {
					UI.DrawText("*" + label + "*", textPos, UI.SubTitle, class_181.field_1718, TextAlignment.Left);
					y += 25;
					var progress = DrawSettingsObject(mod, field.GetValue(settings), pos + new Vector2(15, 0), bgSize, y);
					settingsChanged |= progress.pressed;
					y = progress.curY;
					y += 10;
				}
			}
			y += 40;
		}
		return new DrawProgress { pressed = settingsChanged, curY = y };
	}

	private bool DrawCheckbox(Vector2 pos, string label, bool enabled) {
		Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
		Bounds2 labelBounds = UI.DrawText(label, pos + new Vector2(45f, 13f), UI.SubTitle, class_181.field_1718, TextAlignment.Left);
		if(enabled)
			UI.DrawTexture(class_238.field_1989.field_101.field_773, boxBounds.Min);
		if(boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos())) {
			UI.DrawTexture(class_238.field_1989.field_101.field_774, boxBounds.Min);
			if(!Input.IsLeftClickPressed())
				return false;
			class_238.field_1991.field_1821.method_28(1f);
			return true;
		} else
			UI.DrawTexture(class_238.field_1989.field_101.field_772, boxBounds.Min);
		return false;
	}

	public static void SaveSettings(QuintessentialMod cmod) {
		cmod.ApplySettings();
		ModMeta mod = cmod.Meta;
		var settings = cmod.Settings;
		string name = mod.Name;
		string path = Path.Combine(QuintessentialLoader.PathModSaves, name + ".yaml");
		if(!Directory.Exists(QuintessentialLoader.PathModSaves))
			Directory.CreateDirectory(QuintessentialLoader.PathModSaves);

		using StreamWriter writer = new(path);
		YamlHelper.Serializer.Serialize(writer, settings, QuintessentialLoader.CodeMods.First(c => c.Meta == mod).SettingsType);
	}

	private U FromAttr<T, U>(FieldInfo from, Func<T, U> getter, U def) where T : Attribute {
		T t = from.GetCustomAttributes(true)
				.TakeWhile(att => att is T)
				.Select(att => att as T)
				.FirstOrDefault();
		if(t == null)
			return def;
		else return getter(t);
	}
}
