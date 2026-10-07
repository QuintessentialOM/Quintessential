using System;
using SDL2;

namespace Quintessential;

public class AtomSelectScreen : IScreen{
	
	public string Label;
	public Action<AtomType> OnClick;
	public AtomType Preselected;

	public AtomSelectScreen(string label, Action<AtomType> onClick = null, AtomType preselected = null){
		Label = label;
		OnClick = onClick;
		Preselected = preselected;
	}

	public bool PreventLowerScreenUpdates(){
		return false;
	}

	public void OnOpenOrClose(bool isOpening){
		// Add gray BG
		GameLogic.instance.fadeBackground = true;
	}
	
	public void Reset(){}
	
	public void RenderFrame(float deltaTime){
		// Display a title
		UI.DrawText(Label, (InputManager.screenSize / 2) + new Vector2(0, 170), UI.Title, Color.White, TextAlignment.Center);

		// draw atom options
		var numAtoms = AtomTypes.atoms.Length;
		for(var idx = 0; idx < numAtoms; idx++){
			var type = AtomTypes.atoms[idx];
			if(ClickableAtom((InputManager.screenSize / 2) + new Vector2(-(numAtoms - 1) * 45 + idx * 90, 0), type, true, type.Equals(Preselected))){
				OnClick?.Invoke(type);
				UI.CloseScreen();
			}
		}
		
		// "press esc to CANCEL"
		Bounds2 labelBounds = UI.DrawText("Press ESC to ", (InputManager.screenSize / 2) + new Vector2(-40, -170), UI.SubTitle, UIConsts.lightTextColor, TextAlignment.Center);
		if(InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE) || UI.DrawAndCheckSimpleButton("CANCEL", labelBounds.BottomRight + new Vector2(10, -7), new Vector2(70, (int)labelBounds.Height + 10)))
			UI.HandleCloseButton();
	}
	
	private static bool ClickableAtom(Vector2 pos, AtomType atom, bool selectable, bool selected){
		float alpha = selectable ? 1 : .3f;
		Vector2 centred = (pos - Assets.textures.molecule_editor.atom_outline.size.ToVector2() / 2).Rounded();
		// slot around the atom
		TextureRenderer.Render(selected ? Assets.textures.molecule_editor.atom_selected : Assets.textures.molecule_editor.atom_outline, Color.White.WithAlpha(alpha), centred);
		// draw the atom
		Editor.RenderAtom(atom, pos, 1, alpha, 1, 1, -21, 0, null, null, false);
		// are we hovering over it?
		if(!selectable || Vector2.Distance(pos, InputManager.MousePos()) > 37)
			return false;
		// draw the hovering overlay
		Vector2 outlineCentred = (pos - Assets.textures.molecule_editor.grid_circle_hover.size.ToVector2() / 2).Rounded();
		TextureRenderer.Render(Assets.textures.molecule_editor.grid_circle_hover, outlineCentred);
		// are we clicking?
		if(InputManager.IsClickHeld(MouseButtonType.LeftClick)) {
			// make a sound
			Assets.sounds.click_button.PlaySound(1);
			return true;
		}
		return false;
	}
}