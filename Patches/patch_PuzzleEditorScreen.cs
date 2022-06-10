#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential;

class patch_PuzzleEditorScreen{

	public extern void orig_method_50(float param);

	public void method_50(float param){
		orig_method_50(param);

		if(QApi.CustomPermisions.Count > 0){
			Vector2 size = new(1516 - 152, 922 - 158);

			Vector2 moreButton = Input.ScreenSize() / 2 + new Vector2(+size.X / 2, +size.Y / 2) + new Vector2(-140, -40);
			Bounds2 moreBounds = Bounds2.WithSize(moreButton, new Vector2(36f, 37f));

			if(moreBounds.Contains(Input.MousePos()))
				class_135.method_271(class_238.field_1989.field_101.field_774, Color.White.WithAlpha(0.7f), moreButton);
			else class_135.method_271(class_238.field_1989.field_101.field_772, Color.White.WithAlpha(0.7f), moreButton);
			UI.DrawTexture(class_238.field_1989.field_87.field_668, moreButton);

			if(moreBounds.Contains(Input.MousePos()) && Input.IsLeftClickPressed())
				UI.OpenScreen(new PuzzleEditorExScreen());
		}
	}
}