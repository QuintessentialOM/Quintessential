using System;
using SDL2;

namespace Quintessential;

using AtomTypes = class_175;

public class AtomSelectScreen : IScreen{
	
	public string Label;
	public Action<AtomType> OnClick;
	public AtomType Preselected;

	public AtomSelectScreen(string label, Action<AtomType> onClick = null, AtomType preselected = null){
		Label = label;
		OnClick = onClick;
		Preselected = preselected;
	}

	public bool method_1037(){
		return false;
	}

	public void method_47(bool param_4687){
		// Add gray BG
		GameLogic.field_2434.field_2464 = true;
	}
	
	public void method_48(){}
	
	public void method_50(float param_4686){
		// Display a title
		UI.DrawText(Label, (Input.ScreenSize() / 2) + new Vector2(0, 170), UI.Title, Color.White, TextAlignment.Centred);

		// draw atom options
		var numAtoms = AtomTypes.field_1691.Length;
		for(var idx = 0; idx < numAtoms; idx++){
			var type = AtomTypes.field_1691[idx];
			if(ClickableAtom((Input.ScreenSize() / 2) + new Vector2(-(numAtoms - 1) * 45 + idx * 90, 0), type, true, type.Equals(Preselected))){
				OnClick?.Invoke(type);
				UI.CloseScreen();
			}
		}
		
		// "press esc to CANCEL"
		Bounds2 labelBounds = UI.DrawText("Press ESC to ", (Input.ScreenSize() / 2) + new Vector2(0, -170), UI.SubTitle, class_181.field_1718, TextAlignment.Centred);
		if(Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE) || UI.DrawAndCheckSimpleButton("CANCEL", labelBounds.BottomRight + new Vector2(10, -7), new Vector2(70, (int)labelBounds.Height + 10)))
			UI.HandleCloseButton();
	}
	
	private static bool ClickableAtom(Vector2 pos, AtomType atom, bool selectable, bool selected){
		float alpha = selectable ? 1 : .3f;
		Vector2 centred = (pos - class_238.field_1989.field_89.field_117.field_2056.ToVector2() / 2).Rounded();
		// slot around the atom
		class_135.method_271(selected ? class_238.field_1989.field_89.field_118 : class_238.field_1989.field_89.field_117, Color.White.WithAlpha(alpha), centred);
		// draw the atom
		Editor.method_927(atom, pos, 1, alpha, 1, 1, -21, 0, null, null, false);
		// are we hovering over it?
		if(!selectable || Vector2.Distance(pos, Input.MousePos()) > 37)
			return false;
		// draw the hovering overlay
		Vector2 outlineCentred = (pos - class_238.field_1989.field_89.field_124.field_2056.ToVector2() / 2).Rounded();
		class_135.method_272(class_238.field_1989.field_89.field_124, outlineCentred);
		// are we clicking?
		if(Input.IsLeftClickHeld()){
			// make a sound
			class_238.field_1991.field_1821.method_28(1);
			return true;
		}
		return false;
	}
}