using SDL2;

namespace Quintessential;

/// <summary>
/// Generic info popup screen.
/// </summary>
public class PuzzleEditorExScreen : IScreen {

	private static readonly string Title = "More Options";

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
		UI.DrawText(Title, (Input.ScreenSize() / 2) + new Vector2(0, 300), UI.Title, Color.White, TextAlignment.Centred);
		
		var cursor = Input.ScreenSize() / 2 - new Vector2(100, QApi.CustomPermisions.Count * 20);
		foreach(var permission in QApi.CustomPermisions){
			if(ModsScreen.DrawCheckbox(cursor, permission.Right, false)){

			}
			cursor.Y += 40;
		}

		if(Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE) || UI.DrawAndCheckBoxButton("OK", (Input.ScreenSize() / 2) + new Vector2(-130, -300)))
			UI.CloseScreen();
	}
}