using System.Collections.Generic;
using MonoMod.Utils;

namespace Quintessential;

/// <summary>
/// A collection of UI drawing Util Methods.<br/>
/// For more see <seealso cref="UIUtils"/> and <seealso cref="TextureRenderer"/>.
/// </summary>
public static class UI {

    #region Constants

    /// <summary>
    /// The font used for titles.
    /// </summary>
    public static readonly LocalizedFont Title = Assets.fonts.crimson_21;
    /// <summary>
    /// The font used for general text.
    /// </summary>
	public static readonly LocalizedFont Text = Assets.fonts.crimson_16_5;
    /// <summary>
    /// The font used for subtitles.
    /// </summary>
	public static readonly LocalizedFont SubTitle = Assets.fonts.crimson_13;

    /// <summary>
    /// The main text color.
    /// </summary>
	public static readonly Color TextColor = UIConsts.lightTextColor;

    /// <summary>
    /// A larger version of <c><see cref="Assets.textures.window.background"/></c>
    /// </summary>
	public static readonly Texture BackgroundLarger = AssetLoaderHelper.LoadTexture("textures/background_larger");

    #endregion

    #region Texture drawing methods

    /// <summary>
    /// Draw a <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/> to draw.</param>
    public static void DrawTexture(Texture texture, Vector2 pos) {
		TextureRenderer.Render(texture, pos);
	}

    /// <summary>
    /// Draw a repeating <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/> to draw.</param>
	public static void DrawRepeatingTexture(Texture texture, Vector2 pos, Vector2 size) {
		TextureRenderer.RenderMasked(texture, Color.White, pos, Bounds2.WithSize(pos, size));
	}

    /// <summary>
    /// Draw a resizable <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/> to draw.</param>
	public static void DrawResizableTexture(Texture texture, Vector2 pos, Vector2 size) {
		TextureRenderer.Render9Slice(texture, Color.White, pos, size);
	}

    #endregion

    #region Text drawing methods

    /// <summary>
    /// Draw the text on the screen with the given settings.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="color">The text color.</param>
    /// <param name="alignment">The alingment of the text.</param> // Chaotic Neutral ?
    /// <param name="maxWidth">Maximum line width.</param>
    /// <returns>The bounds of the drawn text.</returns>
    public static Bounds2 DrawText(string text, Vector2 pos, StyleableFont font, Color color, TextAlignment alignment, float maxWidth = float.MaxValue, float ellipsesCutoff = float.MaxValue) {
		return TextureRenderer.RenderText(text, pos, font, color, alignment, 1f, 0.6f, maxWidth, ellipsesCutoff, 0, new Color(), null, int.MaxValue, true, true);
	}

    /// <summary>
    /// Draws a UI title.
    /// </summary>
    /// <param name="text">The translation key for the title.</param>
    /// <param name="a">Whether the title is spaced out.</param>
    /// <param name="b">Whether the title is UPPER-case.</param>
	public static void DrawHeader(string text, Vector2 pos, int width, bool a, bool b) {
		UIUtils.RenderScreenTitle(Translations.Translate(text), pos, width, true, true);
	}

    #endregion

    #region Button drawing methods

    /// <summary>
    /// Draws and checks a close button.
    /// </summary>
    /// <param name="closeButtonOffset">The offset of the button relative to the frame</param>
    /// <returns>True when the button clicked.</returns>
    public static bool DrawAndCheckCloseButton(Vector2 framePos, Vector2 frameSize, Vector2 closeButtonOffset) {
		return UIUtils.CloseButton(framePos, frameSize, frameSize - closeButtonOffset);
	}

    /// <summary>
    /// Draws and checks a button displaying multiple lines of text.
    /// </summary>
    /// <param name="text">The main line of text on the button.</param>
    /// <param name="subtext">The Subtext of the button.</param>
    /// <param name="width">Text overflow width.</param>
    /// <param name="selected">Whether the button is selected</param>
    /// <returns>True when the button clicked.</returns>
    public static bool DrawAndCheckSolutionButton(string text, string subtext, Vector2 pos, int width, bool selected) {
		return UIUtils.SolutionButton(text, subtext == null ? MaybeHelper.empty : Maybe<string>.From(subtext), pos, width, selected).RenderAndCheckIfPressed(true, true);
	}

    /// <summary>
    /// Draws and checks a text button.
    /// </summary>
    /// <param name="text">The label of the button.</param>
    /// <returns>True when the button clicked.</returns>
    public static bool DrawAndCheckBoxButton(string text, Vector2 pos) {
		return UIUtils.TextButton(text, pos).RenderAndCheckIfPressed(true, true);
	}

    /// <summary>
    /// Draws and checks a text button, with a size.
    /// </summary>
    /// <param name="text">The label of the button.</param>
    /// <returns>True when the button clicked.</returns>
    public static bool DrawAndCheckSimpleButton(string text, Vector2 pos, Vector2 size) {
		return UIUtils.class_149.method_348(text, pos, size).RenderAndCheckIfPressed(true, true);
	}

    #endregion

    #region Screen stack

    /// <summary>
    /// Close the <see cref="IScreen"/> without a <see cref="Transition"/>.<br/>
	/// And plays the screen buton click sound.
	/// </summary>
    public static void HandleCloseButton() {
		CloseScreen();
		// Play close sound
		Assets.sounds.ui_modal_close.method_28(1f);
	}

	/// <summary>
	/// Close the <see cref="IScreen"/> without a <see cref="Transition"/>.<br/>
	/// And fade out from black.
	/// </summary>
    public static void CloseScreen() {
		GameLogic.instance.fadeBackground = false;
		GameLogic.instance.PopScreen();
	}

    /// <summary>
    /// Close the <see cref="IScreen"/> without a <see cref="Transition"/>.
    /// </summary>
    public static void InstantCloseScreen() {
		GameLogic.instance.PopScreens(1);
	}

    /// <summary>
    /// Open a <see cref="IScreen"/> without a <see cref="Transition"/>.
    /// </summary>
    public static void OpenScreen(IScreen toOpen) {
		GameLogic.instance.PushScreen(toOpen, MaybeHelper.empty, MaybeHelper.empty);
	}

    #endregion

    #region UI helpers

    /// <summary>
    /// Draw a repeating UI background to the given area.
    /// </summary>
    public static void DrawUiBackground(Vector2 pos, Vector2 size) {
		DrawRepeatingTexture(Assets.textures.window.background, pos, size);
	}

    /// <summary>
    /// Draw a repeating UI background to the given area, using the large texture.
    /// </summary>
    public static void DrawLargeUiBackground(Vector2 pos, Vector2 size) {
		DrawRepeatingTexture(BackgroundLarger, pos, size);
	}

    /// <summary>
    /// Draw a resizable UI window frame around the given area.
    /// </summary>
    public static void DrawUiFrame(Vector2 pos, Vector2 size) {
		DrawResizableTexture(Assets.textures.window.frame, pos, size);
	}

    /// <summary>
    /// Draws and checks a labeled check box.
    /// </summary>
    /// <param name="label">The label of the box.</param>
    /// <param name="enabled">Should the check mark be drawn.</param>
    /// <returns>True when the box is clicked.</returns>
    public static bool DrawCheckbox(Vector2 pos, string label, bool enabled) {
		Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
		Bounds2 labelBounds = DrawText(label, pos + new Vector2(45f, 13f), SubTitle, TextColor, TextAlignment.Left);
		if(enabled)
			DrawTexture(Assets.textures.UI.checkbox_fill, boxBounds.Min);
		if(boxBounds.Contains(InputManager.MousePos()) || labelBounds.Contains(InputManager.MousePos())) {
			DrawTexture(Assets.textures.UI.checkbox_hover, boxBounds.Min);
			if(!InputManager.IsClickPressed(MouseButtonType.LeftClick))
				return false;
            Assets.sounds.click_button.method_28(1f);
			return true;
		}
		DrawTexture(Assets.textures.UI.checkbox, boxBounds.Min);
		return false;
    }

    #endregion

    #region Texture control methods

    /// <summary>
    /// Set the <see cref="Texture"/> offset in the global class.
    /// </summary>
    /// <param name="tex">The <see cref="Texture"/> to set the offset for.</param>
    /// <param name="offset">The offset to set.</param>
    /// <returns>The texture.</returns>
    public static Texture AssignOffset(Texture tex, Vector2 offset){
		new DynamicData(typeof(TextureOffsets)).Get<Dictionary<Texture, Vector2>>("offsets")[tex] = offset;
		return tex;
	}

    /// <summary>
    /// Set the <see cref="Texture"/> scale in the global class.
    /// </summary>
    /// <param name="tex">The <see cref="Texture"/> to set the scale for.</param>
    /// <param name="scale">The scale to set.</param>
    /// <returns>The texture.</returns>
    public static Texture AssignScale(Texture tex, Vector2 scale) {
		new DynamicData(typeof(TextureOffsets)).Get<Dictionary<Texture, Vector2>>("scales")[tex] = scale;
		return tex;
	}

	#endregion
}
