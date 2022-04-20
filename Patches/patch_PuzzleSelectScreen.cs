using MonoMod.Utils;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_PuzzleSelectScreen {

	private static int currentCampaign = 0;

	public extern void orig_method_50(float time);

	public void method_50(float time) {
		orig_method_50(time);
		if(QuintessentialSettings.Instance.EnableCustomCampaigns && QuintessentialLoader.AllCampaigns.Count > 1) {
			var dyn = new DynamicData(typeof(PuzzleSelectScreen), this);
			float y1 = class_162.method_417(-220f, 0.0f, dyn.Get<float>("field_2937"));
			// add campaign change buttons
			Vector2 leftPos = new(class_115.field_1433.X / 2f - 305, 30 + y1);
			Vector2 rightPos = new(class_115.field_1433.X / 2f + 269, 30 + y1);
			Bounds2 leftBounds = Bounds2.WithSize(leftPos, new Vector2(36f, 37f));
			Bounds2 rightBounds = Bounds2.WithSize(rightPos, new Vector2(36f, 37f));
			if(leftBounds.Contains(Input.MousePos()))
				class_135.method_271(class_238.field_1989.field_101.field_774, Color.White.WithAlpha(0.7f), leftPos);
			else class_135.method_271(class_238.field_1989.field_101.field_772, Color.White.WithAlpha(0.7f), leftPos);
			if(rightBounds.Contains(Input.MousePos()))
				class_135.method_271(class_238.field_1989.field_101.field_774, Color.White.WithAlpha(0.7f), rightPos);
			else class_135.method_271(class_238.field_1989.field_101.field_772, Color.White.WithAlpha(0.7f), rightPos);
			UI.DrawTexture(class_238.field_1989.field_87.field_669, leftPos);
			UI.DrawTexture(class_238.field_1989.field_87.field_668, rightPos);
			// show the currently displayed campaign
			UI.DrawText(((patch_Campaign)(object)QuintessentialLoader.AllCampaigns[currentCampaign]).QuintTitle, new Vector2(Input.ScreenSize().X / 2f, 20 + y1), UI.Text, Color.LightGray, TextAlignment.Centred);
			// reopen the menu if clicked
			var settings = QuintessentialSettings.Instance.SwitcherSettings;
			bool keyLeft = settings.SwitchCampaignLeft.Pressed();
			bool keyRight = settings.SwitchCampaignRight.Pressed();

			if((leftBounds.Contains(Input.MousePos()) && Input.IsLeftClickPressed()) || keyLeft) {
				class_238.field_1991.field_1821.method_28(1f);
				currentCampaign = System.Math.Abs((currentCampaign - 1) % QuintessentialLoader.AllCampaigns.Count);
				Campaigns.field_2330 = QuintessentialLoader.AllCampaigns[currentCampaign];
				Campaigns.field_2331[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
				UI.InstantCloseScreen();
				UI.OpenScreen(new PuzzleSelectScreen());
			} else if((rightBounds.Contains(Input.MousePos()) && Input.IsLeftClickPressed()) || keyRight) {
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