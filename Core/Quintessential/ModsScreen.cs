using Quintessential.Internal;
using Quintessential.Settings;
using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quintessential;

class ModsScreen : IScreen {

	private const int modButtonWidth = 300;
	private static readonly Texture verticalBarCentreTall = AssetLoaderHelper.LoadTexture("textures/vertical_bar_centre_tall");

    private int selectedIndex = 0;
    private ModMeta Selected {
        get => QuintessentialLoader.Mods[Utils.Clamp(selectedIndex, 0, QuintessentialLoader.Mods.Count - 1)];
    }
	private Scrollbar modsListScrollbar = new();

	private struct DrawProgress {
		public bool pressed;
		public float curY;
	}

	public bool PreventLowerScreenUpdates() {
		return false;
	}

	public void OnOpenOrClose(bool isOpening) {

	}

	public void Reset() {

	}

	// update & render
	public void RenderFrame(float deltaTime) {
		Vector2 size = new(1220, 1000);
		Vector2 pos = (InputManager.screenSize / 2 - size / 2).Rounded();
		Vector2 bgPos = pos + new Vector2(78, 88);
		Vector2 bgSize = size - new Vector2(78 * 2, 77 * 2);

        UI.DrawLargeUiBackground(bgPos, bgSize);
        TextureRenderer.Render9Slice(Assets.textures.window.frame, Color.White, pos, size);
        TextureRenderer.Render(verticalBarCentreTall, pos + new Vector2(modButtonWidth + 130, 76f));

		if(UI.DrawAndCheckCloseButton(pos, size, new Vector2(104, 98)))
			UI.HandleCloseButton();

        if (InputManager.IsKeyHeld(SDL.SDLKey.SDLK_DOWN)) {
            selectedIndex = Math.Min(selectedIndex + 1, QuintessentialLoader.Mods.Count - 1);
        } else if (InputManager.IsKeyHeld(SDL.SDLKey.SDLK_UP)) {
            selectedIndex = Math.Max(selectedIndex - 1, 0);
        }

        // draw mod buttons
        using (var _ = modsListScrollbar.RenderScrollbar(bgPos + new Vector2(0, 5), new(modButtonWidth + 60, (int)bgSize.Y - 10), 0, -30)){
			// clear scroll zone
			class_226.method_600(Color.Transparent);
			
			int y = -(int)modsListScrollbar.scrollOffset;
			UI.DrawHeader("Mods", new Vector2(20, size.Y - 200 - y), modButtonWidth, true, true);

            for (int i = 0; i < QuintessentialLoader.Mods.Count; i++) {
                ModMeta mod = QuintessentialLoader.Mods[i];
                if (UI.DrawAndCheckSolutionButton(Translations.Translate(mod.ModId), mod.Version.ToString(), new Vector2(20, size.Y - 290 - y), modButtonWidth, Selected == mod))
                    selectedIndex = i;
                y += 70;
            }
			
			// expand the scroll area to cover the entire displayed area
			modsListScrollbar.SetHeightAndClamp(y + 212);
		}
		
		// draw mod options panel
		DrawModOptions(pos + new Vector2(modButtonWidth + 160, -10), size - new Vector2(160, 10), Selected);
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
		if (hasIcon)
			TextureRenderer.Render(mod.IconCache ??= AssetLoaderHelper.LoadTexture(mod.Icon), pos + new Vector2(20, bgSize.Y - 99f - 100));

        UI.DrawText(Translations.Translate(mod.ModId), titlePos + new Vector2(20, bgSize.Y - 99f), UI.Title, UI.TextColor, TextAlignment.Left);
		string ver = mod.Version.ToString();
        UI.DrawText(mod.ModId.EscapeFormatting() + " - " + ver, titlePos + new Vector2(20, bgSize.Y - 130f), UI.Text, Color.LightGray, TextAlignment.Left);

		var modDescription = Translations.Translate(mod.ModId + ".description");
        if (modDescription != (mod.ModId + ".description")) { // Possibly broken with psudo language & missing english translation
			var desc = UI.DrawText(modDescription, pos + new Vector2(20, bgSize.Y - 170f - (hasIcon ? 70 : 0)), UI.Text, UI.TextColor, TextAlignment.Left, maxWidth: 460);
			return desc.Height + 80;
		}
		return 20;
	}

	private bool DrawModSettings(QuintessentialMod mod, Vector2 pos, Vector2 bgSize) {
		var settings = mod.Settings;
		return settings == null ? false : DrawSettingsObject(mod, settings, pos, bgSize, 170).pressed;
	}

	private DrawProgress DrawSettingsObject(QuintessentialMod mod, object settings, Vector2 pos, Vector2 bgSize, float startY) {
		float y = startY;
		bool settingsChanged = false;
		if(settings == null)
			return new DrawProgress { pressed = false, curY = 0 };
		foreach(var field in settings.GetType().GetFields()) {
			if(field.IsStatic)
				continue;

			LocString label = Translations.Translate(field.GetCustomAttribute<SettingsLabelKey>()?.Label ?? field.Name);

            if (field.FieldType == typeof(bool)) {
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
				var text = !string.IsNullOrWhiteSpace(key.Key) ? key.Key : QuintessentialCore.Instance.Translate("display_text.empty_keybind");
                if (UI.DrawAndCheckSimpleButton(text, labelBounds.BottomRight + new Vector2(10, 0), new Vector2(50, (int)labelBounds.Height)))
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
			// TODO Add enum Settings
			//} else if (typeof(Enum).IsAssignableFrom(field.FieldType)) {
			//	var fileds = field.FieldType.GetFields()[1..];
			//	var textPos = pos + new Vector2(20, bgSize.Y - y + 5);
			//	int value = (int)field.GetValue(settings);
			//	string selectedName = "--##--";
			//	string[] fieldNames = new string[fileds.Length];
			//	for (int i = 0; i < fileds.Length; i++) {
			//		fieldNames[i] = fileds[i].Name;
			//		if (value == (int)fileds[i].GetRawConstantValue()) selectedName = fieldNames[i];
			//	}
			}
            y += 40;
		}
		return new DrawProgress { pressed = settingsChanged, curY = y };
	}

	public static void SaveSettings(QuintessentialMod mod){
		mod.ApplySettings();
		ModMeta meta = mod.Meta;
		object settings = mod.Settings;
		string id = meta.ModId;
		string path = Path.Combine(QuintessentialLoader.PathModSaves, id + ".yaml");
		if(!Directory.Exists(QuintessentialLoader.PathModSaves))
			Directory.CreateDirectory(QuintessentialLoader.PathModSaves);

		using StreamWriter writer = new(path);
		YamlHelper.Serializer.Serialize(writer, settings, QuintessentialLoader.CodeMods.First(c => c.Meta == meta).SettingsType);
	}
}