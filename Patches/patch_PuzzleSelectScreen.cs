using MonoMod.Utils;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_PuzzleSelectScreen {

	private static int currentCampaign = 0;

	public extern void orig_method_50(float time);

	public void method_50(float time) {
		orig_method_50(time);
		if(QuintessentialLoader.AllCampaigns.Count > 1) {
			var dyn = new DynamicData(typeof(PuzzleSelectScreen), this);
			float y1 = class_162.method_417(-220f, 0.0f, dyn.Get<float>("field_2937"));
			// add campaign change buttons
			Vector2 leftPos = new Vector2(class_115.field_1433.X / 2f - 305, 30 + y1);
			Vector2 rightPos = new Vector2(class_115.field_1433.X / 2f + 269, 30 + y1);
			Bounds2 leftBounds = Bounds2.WithSize(leftPos, new Vector2(36f, 37f));
			Bounds2 rightBounds = Bounds2.WithSize(rightPos, new Vector2(36f, 37f));
			if(leftBounds.Contains(class_115.method_202()))
				class_135.method_271(class_238.field_1989.field_101.field_774, Color.White.WithAlpha(0.7f), leftPos);
			else class_135.method_271(class_238.field_1989.field_101.field_772, Color.White.WithAlpha(0.7f), leftPos);
			if(rightBounds.Contains(class_115.method_202()))
				class_135.method_271(class_238.field_1989.field_101.field_774, Color.White.WithAlpha(0.7f), rightPos);
			else class_135.method_271(class_238.field_1989.field_101.field_772, Color.White.WithAlpha(0.7f), rightPos);
			class_135.method_272(class_238.field_1989.field_87.field_669, leftPos);
			class_135.method_272(class_238.field_1989.field_87.field_668, rightPos);
			// show the currently displayed campaign
			class_135.method_290(((patch_Campaign)(object)QuintessentialLoader.AllCampaigns[currentCampaign]).QuintTitle, new Vector2(class_115.field_1433.X / 2f, 20 + y1), class_238.field_1990.field_2145, Color.LightGray, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, true, true);
			// reopen the menu if clicked
			if(class_115.method_206((enum_142)1)) {
				if(leftBounds.Contains(class_115.method_202())) {
					class_238.field_1991.field_1821.method_28(1f);
					currentCampaign = System.Math.Abs((currentCampaign - 1) % QuintessentialLoader.AllCampaigns.Count);
					Campaigns.field_2330 = QuintessentialLoader.AllCampaigns[currentCampaign];
					Campaigns.field_2331[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
					UI.InstantCloseScreen();
					UI.OpenScreen(new PuzzleSelectScreen());
				} else if(rightBounds.Contains(class_115.method_202())) {
					class_238.field_1991.field_1821.method_28(1f);
					currentCampaign = (currentCampaign + 1) % QuintessentialLoader.AllCampaigns.Count;
					Campaigns.field_2330 = QuintessentialLoader.AllCampaigns[currentCampaign];
					Campaigns.field_2331[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
					UI.InstantCloseScreen();
					UI.OpenScreen(new PuzzleSelectScreen());
				}
			}
		}
	}
}