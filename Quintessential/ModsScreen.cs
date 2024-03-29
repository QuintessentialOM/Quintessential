﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Quintessential.Settings;

namespace Quintessential;

using Scrollbar = class_262;

class ModsScreen : IScreen {

	private const int modButtonWidth = 300;
	private static readonly class_256 verticalBarCentreTall = class_235.method_615("Quintessential/vertical_bar_centre_tall");
	
	private ModMeta selected = QuintessentialLoader.QuintessentialModMeta;
	private Scrollbar modsListScrollbar = new();

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
	public void method_50(float time) {
		Vector2 size = new(1220, 1000);
		Vector2 pos = (Input.ScreenSize() / 2 - size / 2).Rounded();
		Vector2 bgPos = pos + new Vector2(78, 88);
		Vector2 bgSize = size - new Vector2(78 * 2, 77 * 2);

		UI.DrawLargeUiBackground(bgPos, bgSize);
		UI.DrawUiFrame(pos, size);
		UI.DrawTexture(verticalBarCentreTall, pos + new Vector2(modButtonWidth + 130, 76f));

		if(UI.DrawAndCheckCloseButton(pos, size, new Vector2(104, 98)))
			UI.HandleCloseButton();

		// draw mod buttons
		using(var _ = modsListScrollbar.method_709(bgPos + new Vector2(0, 5), new(modButtonWidth + 60, (int)bgSize.Y - 10), 0, -30)){
			// clear scroll zone
			class_226.method_600(Color.Transparent);
			
			int y = -(int)modsListScrollbar.field_2078;
			UI.DrawHeader("Mods", new Vector2(20, size.Y - 200 - y), modButtonWidth, true, true);
			
			if(UI.DrawAndCheckSolutionButton("Quintessential", $"{QuintessentialLoader.VersionString} ({QuintessentialLoader.VersionNumber})", new Vector2(20, size.Y - 285 - y), modButtonWidth, selected == QuintessentialLoader.QuintessentialModMeta))
				selected = QuintessentialLoader.QuintessentialModMeta;
			class_135.method_275(class_238.field_1989.field_102.field_822, Color.White, Bounds2.WithSize(new Vector2(20, size.Y - 305 - y), new Vector2(modButtonWidth, 3f)));
			y += 100;
			foreach(var mod in QuintessentialLoader.Mods)
				if(mod != QuintessentialLoader.QuintessentialModMeta){
					if(UI.DrawAndCheckSolutionButton(mod.Title ?? mod.Name, mod.Version.ToString(), new Vector2(20, size.Y - 290 - y), modButtonWidth, selected == mod))
						selected = mod;
					y += 70;
				}
			
			// expand the scroll area to cover the entire displayed area
			modsListScrollbar.method_707(y + 132);
		}
		
		// draw mod options panel
		DrawModOptions(pos + new Vector2(modButtonWidth + 160, -10), size - new Vector2(160, 10), selected);
	}

	private void DrawModOptions(Vector2 pos, Vector2 size, ModMeta mod) {
		float descHeight = DrawModLabel(mod, pos, size);
		foreach(var cmod in QuintessentialLoader.CodeMods)
			if(cmod.Meta == mod)
				if(DrawModSettings(cmod, pos - new Vector2(0, descHeight), size))
					SaveSettings(cmod);
	}

	private float DrawModLabel(ModMeta mod, Vector2 pos, Vector2 bgSize){
		bool hasIcon = !string.IsNullOrWhiteSpace(mod.Icon);
		Vector2 titlePos = hasIcon ? pos + new Vector2(140, -30) : pos;
		if(mod.Icon != null)
			UI.DrawTexture(mod.IconCache ??= class_235.method_615(mod.Icon), pos + new Vector2(20, bgSize.Y - 99f - 100));
		UI.DrawText(mod.Title ?? mod.Name, titlePos + new Vector2(20, bgSize.Y - 99f), UI.Title, UI.TextColor, TextAlignment.Left);
		string ver = mod.Version.ToString();
		if(mod.Title != null)
			ver = mod.Name + " - " + ver;
		UI.DrawText(ver, titlePos + new Vector2(20, bgSize.Y - 130f), UI.Text, Color.LightGray, TextAlignment.Left);
		if(mod.Desc != null) {
			var desc = UI.DrawText(mod.Desc, pos + new Vector2(20, bgSize.Y - 170f - (hasIcon ? 70 : 0)), UI.Text, UI.TextColor, TextAlignment.Left, maxWidth: 460);
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

			string label = field.GetCustomAttribute<SettingsLabel>()?.Label ?? field.Name;

			if(field.FieldType == typeof(bool)) {
				if(UI.DrawCheckbox(pos + new Vector2(20, bgSize.Y - y), label, (bool)field.GetValue(settings))) {
					field.SetValue(settings, !(bool)field.GetValue(settings));
					settingsChanged = true;
				}
			} else if(field.FieldType == typeof(SettingsButton)) {
				if(UI.DrawAndCheckBoxButton(label, pos + new Vector2(20, bgSize.Y - y - 15)))
					((SettingsButton)field.GetValue(settings))();
				y += 20;
			} else if(field.FieldType == typeof(Keybinding)) {
				Keybinding key = (Keybinding)field.GetValue(settings);
				Bounds2 labelBounds = UI.DrawText(label + ": " + key.ControlKeysText(), pos + new Vector2(20, bgSize.Y - y - 15), UI.SubTitle, UI.TextColor, TextAlignment.Left);
				var text = !string.IsNullOrWhiteSpace(key.Key) ? key.Key : "None";
				if(UI.DrawAndCheckSimpleButton(text, labelBounds.BottomRight + new Vector2(10, 0), new Vector2(50, (int)labelBounds.Height)))
					UI.OpenScreen(new ChangeKeybindScreen(key, label, mod));
				y += 20;
			} else if(typeof(SettingsGroup).IsAssignableFrom(field.FieldType)) {
				SettingsGroup group = (SettingsGroup)field.GetValue(settings);
				var textPos = pos + new Vector2(20, bgSize.Y - y + 5);
				if(group.Enabled) {
					UI.DrawText("*" + label + "*", textPos, UI.SubTitle, UI.TextColor, TextAlignment.Left);
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

	[Obsolete("Use UI.DrawCheckbox instead")]
	public static bool DrawCheckbox(Vector2 pos, string label, bool enabled) {
		Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
		Bounds2 labelBounds = UI.DrawText(label, pos + new Vector2(45f, 13f), UI.SubTitle, UI.TextColor, TextAlignment.Left);
		if(enabled)
			UI.DrawTexture(class_238.field_1989.field_101.field_773, boxBounds.Min);
		if(boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos())) {
			UI.DrawTexture(class_238.field_1989.field_101.field_774, boxBounds.Min);
			if(!Input.IsLeftClickPressed())
				return false;
			class_238.field_1991.field_1821.method_28(1f);
			return true;
		}
		UI.DrawTexture(class_238.field_1989.field_101.field_772, boxBounds.Min);
		return false;
	}

	public static void SaveSettings(QuintessentialMod mod){
		mod.ApplySettings();
		ModMeta meta = mod.Meta;
		object settings = mod.Settings;
		string name = meta.Name;
		string path = Path.Combine(QuintessentialLoader.PathModSaves, name + ".yaml");
		if(!Directory.Exists(QuintessentialLoader.PathModSaves))
			Directory.CreateDirectory(QuintessentialLoader.PathModSaves);

		using StreamWriter writer = new(path);
		YamlHelper.Serializer.Serialize(writer, settings, QuintessentialLoader.CodeMods.First(c => c.Meta == meta).SettingsType);
	}
}