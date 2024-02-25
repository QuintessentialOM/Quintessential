namespace Quintessential;

using OMDraw = class_135;
using OMUI = class_140;
using OMFont = class_1;
using OMAssets = class_238;
using OMTexture = class_256;

public static class UI {

	#region Constants

	public static readonly OMFont Title = OMAssets.field_1990.field_2146;
	public static readonly OMFont Text = OMAssets.field_1990.field_2145;
	public static readonly OMFont SubTitle = OMAssets.field_1990.field_2143;

	public static readonly Color TextColor = class_181.field_1718;

	public static readonly OMTexture BackgroundLarger = class_235.method_615("Quintessential/background_larger");

	#endregion

	#region Texture drawing methods

	public static void DrawTexture(OMTexture texture, Vector2 pos) {
		OMDraw.method_272(texture, pos);
	}

	public static void DrawRepeatingTexture(OMTexture texture, Vector2 pos, Vector2 size) {
		OMDraw.method_268(texture, Color.White, pos, Bounds2.WithSize(pos, size));
	}

	public static void DrawResizableTexture(OMTexture texture, Vector2 pos, Vector2 size) {
		OMDraw.method_276(texture, Color.White, pos, size);
	}

	#endregion

	#region Text drawing methods

	public static Bounds2 DrawText(string text, Vector2 pos, OMFont font, Color color, TextAlignment alignment, float maxWidth = float.MaxValue, float ellipsesCutoff = float.MaxValue) {
		return OMDraw.method_290(text, pos, font, color, (enum_0)(int)alignment, 1f, 0.6f, maxWidth, ellipsesCutoff, 0, new Color(), (OMTexture)null, int.MaxValue, true, true);
	}

	public static void DrawHeader(string text, Vector2 pos, int width, bool a, bool b) {
		OMUI.method_317(class_134.method_253(text, string.Empty), pos, width, true, true);
	}

	#endregion

	#region Button drawing methods

	public static bool DrawAndCheckCloseButton(Vector2 framePos, Vector2 frameSize, Vector2 closeButtonOffset) {
		return OMUI.method_323(framePos, frameSize, frameSize - closeButtonOffset);
	}

	public static bool DrawAndCheckSolutionButton(string text, string subtext, Vector2 pos, int width, bool selected) {
		return OMUI.method_315(text, subtext == null ? struct_18.field_1431 : Maybe<string>.method_1089(subtext), pos, width, selected).method_824(true, true);
	}

	public static bool DrawAndCheckBoxButton(string text, Vector2 pos) {
		return OMUI.method_314(text, pos).method_824(true, true);
	}

	public static bool DrawAndCheckSimpleButton(string text, Vector2 pos, Vector2 size) {
		return OMUI.class_149.method_348(text, pos, size).method_824(true, true);
	}

	#endregion

	#region Screen stack

	public static void HandleCloseButton() {
		CloseScreen();
		// Play close sound
		OMAssets.field_1991.field_1873.method_28(1f);
	}

	public static void CloseScreen() {
		GameLogic.field_2434.field_2464 = false;
		GameLogic.field_2434.method_949();
	}

	public static void InstantCloseScreen() {
		GameLogic.field_2434.method_950(1);
	}

	public static void OpenScreen(IScreen toOpen) {
		GameLogic.field_2434.method_945(toOpen, struct_18.field_1431, struct_18.field_1431);
	}

	#endregion

	#region UI helpers

	public static void DrawUiBackground(Vector2 pos, Vector2 size) {
		DrawRepeatingTexture(OMAssets.field_1989.field_102.field_810, pos, size);
	}
	
	public static void DrawLargeUiBackground(Vector2 pos, Vector2 size) {
		DrawRepeatingTexture(BackgroundLarger, pos, size);
	}

	public static void DrawUiFrame(Vector2 pos, Vector2 size) {
		DrawResizableTexture(OMAssets.field_1989.field_102.field_817, pos, size);
	}

	public static bool DrawCheckbox(Vector2 pos, string label, bool enabled) {
		Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
		Bounds2 labelBounds = DrawText(label, pos + new Vector2(45f, 13f), SubTitle, TextColor, TextAlignment.Left);
		if(enabled)
			DrawTexture(class_238.field_1989.field_101.field_773, boxBounds.Min);
		if(boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos())) {
			DrawTexture(class_238.field_1989.field_101.field_774, boxBounds.Min);
			if(!Input.IsLeftClickPressed())
				return false;
			class_238.field_1991.field_1821.method_28(1f);
			return true;
		}
		DrawTexture(class_238.field_1989.field_101.field_772, boxBounds.Min);
		return false;
	}
	
	#endregion
}

public enum TextAlignment {
	Left, Centred, Right
}
