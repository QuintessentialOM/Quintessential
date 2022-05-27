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

	public bool method_1037() {
		return false;
	}

	public void method_47(bool param_4687) {
		// Add gray BG
		GameLogic.field_2434.field_2464 = true;
	}

	public void method_48() {
		
	}

	public void method_50(float param_4686) {
		UI.DrawText(Title, (Input.ScreenSize() / 2) + new Vector2(0, 120), UI.Title, Color.White, TextAlignment.Centred);
		UI.DrawText(Tooltip, Input.ScreenSize() / 2, UI.SubTitle, class_181.field_1718, TextAlignment.Centred);
		if(UI.DrawAndCheckBoxButton("OK", (Input.ScreenSize() / 2) + new Vector2(-130, -160)))
			UI.CloseScreen();
	}
}