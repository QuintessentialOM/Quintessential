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

	internal static MessageBoxScreenEx textbox(Bounds2 bounds, string title, string initialText, string confirmText, Action<string> onConfirm){
		MessageBoxScreenEx ret = new(){
			bounds = bounds,
			title = title,
			text = initialText,
			confirmText = confirmText,
			confirmable = true,
			cancelText = "Cancel",
			isTextbox = true
		};
		ret.onConfirm = () => onConfirm(ret.text);
		return ret;
	}

	public bool method_1037() => false;

	public void method_50(float deltaTime){
		if(field_2624){
			class_135.method_268(class_238.field_1989.field_102.field_819, Color.White, bounds.Min, Bounds2.WithSize(bounds.Min.X, bounds.Min.Y, bounds.Width - 27f, bounds.Height));
			class_135.method_268(class_238.field_1989.field_102.field_819, Color.White, bounds.Min, Bounds2.WithSize(bounds.Max.X - 27f, bounds.Min.Y, 27f, bounds.Height - 27f));
		}else
			class_135.method_268(class_238.field_1989.field_102.field_819, Color.White, bounds.Min, bounds);

		Vector2 centre = bounds.Center.Rounded();
		if(isTextbox)
			centre.Y -= 34f;
		if(isTextbox){
			class_135.method_290(title, centre + new Vector2(4f, 100f), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			class_135.method_275(class_238.field_1989.field_101.field_778, Color.White, Bounds2.WithSize(centre + new Vector2(-265f, 24f), new Vector2(532f, 48f)));
			Bounds2 bounds2 = class_135.method_290(text.Length == 0 ? " " : text, centre + new Vector2(0.0f, 43f), class_238.field_1990.field_2144, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, true, true);
			cursorBlink = (cursorBlink + deltaTime) % cursorBlinkSpeed;
			if(cursorBlink < cursorBlinkSpeed / 2.0)
				class_135.method_280(class_181.field_1718, Bounds2.WithSize(bounds2.BottomRight + new Vector2(2f, 1f), new Vector2(2f, 22f)));
			char upper = /*char.ToUpper(*/class_115.method_201()/*)*/;
			if(upper != char.MinValue){
				text = (text + upper).method_437();
				cursorBlink = 0.0f;
			}

			if(class_115.method_200(SDL.enum_160.SDLK_BACKSPACE) && text.Length > 0){
				if(class_115.method_193((enum_143)1)){
					text = text.TrimEnd();
					text = text.Substring(0, text.LastIndexOf(' ') + 1);
				}else
					text = text.Substring(0, text.Length - 1);

				cursorBlink = 0.0f;
			}
		}else if(field_2626.method_1085()){
			class_135.method_290(title, centre + new Vector2(0.0f, 70f), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			class_135.method_290(field_2626.method_1087(), centre + new Vector2(0.0f, 30f), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		}else
			class_135.method_290(title, centre + new Vector2(0.0f, 30f), class_238.field_1990.field_2145, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);

		ButtonDrawingLogic buttonDrawingLogic;
		if(confirmable){
			bool textValid = !isTextbox || text.Length > 0;
			bool pressedEnter = textValid && class_115.method_196();
			buttonDrawingLogic = class_140.method_314(confirmText, centre + new Vector2(15f, -50f));
			if(buttonDrawingLogic.method_824(textValid, true) || pressedEnter){
				onConfirm();
				UI.CloseScreen();
				class_238.field_1991.field_1821.method_28(1f);
			}
		}

		int x = confirmable ? -265 : -127;
		buttonDrawingLogic = class_140.method_314(cancelText, centre + new Vector2(x, -50f));
		if(buttonDrawingLogic.method_824(true, true) || class_115.method_198(SDL.enum_160.SDLK_ESCAPE)){
			onCancel();
			UI.CloseScreen();
			class_238.field_1991.field_1821.method_28(1f);
		}

		if(bounds.Contains(Input.MousePos()) || !Input.IsLeftClickPressed())
			return;
		onCancel();
		UI.CloseScreen();
		class_238.field_1991.field_1821.method_28(1f);
	}

	public void method_47(bool param_4687){}

	public void method_48(){}
}