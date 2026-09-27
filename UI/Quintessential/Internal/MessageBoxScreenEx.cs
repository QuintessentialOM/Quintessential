using System;
using SDL2;

namespace Quintessential.Internal;


// TODO: replace with more generic free text input
internal sealed class MessageBoxScreenEx : IScreen{
	private const float cursorBlinkSpeed = 1.2f;
	private Bounds2 bounds;
	private bool field_2624 = true;
	private string title;
	private Maybe<string> field_2626;
	private string confirmText;
	private bool confirmable;
	private string cancelText;
	private bool isTextbox;
	private string text;
	private float cursorBlink;
	private Action onConfirm = () => {};
	private Action onCancel = () => {};

	internal static MessageBoxScreenEx Textbox(Bounds2 bounds, string title, string initialText, string confirmText, Action<string> onConfirm){
		MessageBoxScreenEx ret = new(){
			bounds = bounds,
			title = title,
			text = initialText,
			confirmText = confirmText,
			confirmable = true,
			cancelText = Translations.Translate("Cancel"),
			isTextbox = true
		};
		ret.onConfirm = () => onConfirm(ret.text);
		return ret;
	}

	public bool PreventLowerScreenUpdates() => false;

	public void RenderFrame(float deltaTime){
		if(field_2624){
			TextureRenderer.RenderMasked(Assets.textures.window.overlay, Color.White, bounds.Min, Bounds2.WithSize(bounds.Min.X, bounds.Min.Y, bounds.Width - 27f, bounds.Height));
			TextureRenderer.RenderMasked(Assets.textures.window.overlay, Color.White, bounds.Min, Bounds2.WithSize(bounds.Max.X - 27f, bounds.Min.Y, 27f, bounds.Height - 27f));
		}else
			TextureRenderer.RenderMasked(Assets.textures.window.overlay, Color.White, bounds.Min, bounds);

		Vector2 centre = bounds.Center.Rounded();
		if(isTextbox)
			centre.Y -= 34f;
		if(isTextbox){
			TextureRenderer.RenderText(title, centre + new Vector2(4f, 100f), Assets.fonts.crimson_16_5, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			TextureRenderer.Render9Slice(Assets.textures.UI.dropdown_list, Color.White, Bounds2.WithSize(centre + new Vector2(-265f, 24f), new Vector2(532f, 48f)));
			Bounds2 bounds2 = TextureRenderer.RenderText(text.Length == 0 ? " " : text, centre + new Vector2(0.0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, true, true);
			cursorBlink = (cursorBlink + deltaTime) % cursorBlinkSpeed;
			if(cursorBlink < cursorBlinkSpeed / 2.0)
				TextureRenderer.RenderColor(UIConsts.lightTextColor, Bounds2.WithSize(bounds2.BottomRight + new Vector2(2f, 1f), new Vector2(2f, 22f)));
			char upper = /*char.ToUpper(*/InputManager.GetCharByInput()/*)*/;
			if(upper != char.MinValue){
				text = (text + upper).RemoveStyleFormat();
				cursorBlink = 0.0f;
			}
            
            if (InputManager.IsKeyHeld(SDL.SDLKey.SDLK_BACKSPACE) && text.Length > 0) {
				if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
					text = text.TrimEnd();
					text = text[..(text.LastIndexOf(' ') + 1)];
				} else
					text = text[..^1];

				cursorBlink = 0.0f;
			}
		}else if(field_2626.HasValue()){
			TextureRenderer.RenderText(title, centre + new Vector2(0.0f, 70f), Assets.fonts.crimson_16_5, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			TextureRenderer.RenderText(field_2626.GetValue(), centre + new Vector2(0.0f, 30f), Assets.fonts.crimson_16_5, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		}else
			TextureRenderer.RenderText(title, centre + new Vector2(0.0f, 30f), Assets.fonts.crimson_16_5, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);

		ButtonDrawingLogic buttonDrawingLogic;
		if(confirmable){
			bool textValid = !isTextbox || text.Length > 0;
			bool pressedEnter = textValid && InputManager.IsEnterPressed();
			buttonDrawingLogic = UIUtils.TextButton(confirmText, centre + new Vector2(15f, -50f));
			if(buttonDrawingLogic.RenderAndCheckIfPressed(textValid, true) || pressedEnter){
				onConfirm();
				UI.CloseScreen();
				Assets.sounds.click_button.method_28(1f);
			}
		}

		int x = confirmable ? -265 : -127;
		buttonDrawingLogic = UIUtils.TextButton(cancelText, centre + new Vector2(x, -50f));
		if(buttonDrawingLogic.RenderAndCheckIfPressed(true, true) || InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE)){
			onCancel();
			UI.CloseScreen();
			Assets.sounds.click_button.method_28(1f);
		}

		if(bounds.Contains(InputManager.MousePos()) || !InputManager.IsClickPressed(MouseButtonType.LeftClick))
			return;
		onCancel();
		UI.CloseScreen();
		Assets.sounds.click_button.method_28(1f);
	}

	public void OnOpenOrClose(bool isOpening){}

	public void Reset(){}
}