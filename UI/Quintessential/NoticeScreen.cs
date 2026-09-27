using SDL2;

namespace Quintessential;

/// <summary>
/// Generic info popup screen.
/// </summary>
public class NoticeScreen : IScreen {

	private readonly string Title, Tooltip;

    public NoticeScreen(string title, string tooltip) {
		Title = title;
		Tooltip = tooltip;
	}

	public bool PreventLowerScreenUpdates() {
		return false;
	}

	public void OnOpenOrClose(bool isOpening) {
		// Add gray BG
		GameLogic.instance.fadeBackground = true;
	}

	public void Reset() {
		
	}

	public void RenderFrame(float deltaTime) {
		UI.DrawText(Title, (InputManager.screenSize / 2) + new Vector2(0, 120), UI.Title, Color.White, TextAlignment.Center);
		UI.DrawText(Tooltip, InputManager.screenSize / 2, UI.SubTitle, UIConsts.lightTextColor, TextAlignment.Center);
		if(InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE) || UI.DrawAndCheckBoxButton("OK", (InputManager.screenSize / 2) + new Vector2(-130, -160)))
			UI.CloseScreen();
	}
}