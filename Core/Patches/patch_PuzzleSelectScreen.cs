using MonoMod.Utils;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_PuzzleSelectScreen {

	private static int currentCampaign = 0;

	public extern void orig_RenderFrame(float deltaTime);

	public void RenderFrame(float deltaTime) {
        orig_RenderFrame(deltaTime);
		if(QuintessentialCoreSettings.Instance.EnableCustomCampaigns && QuintessentialLoader.AllCampaigns.Count > 1) {
			var dyn = new DynamicData(typeof(PuzzleSelectScreen), this);
			float y1 = Utils.InterpolateCos(-220f, 0.0f, dyn.Get<float>("chapterUIFadeTimer"));
			// add campaign change buttons
			Vector2 leftPos = new(InputManager.screenSize.X / 2f - 305, 30 + y1);
			Vector2 rightPos = new(InputManager.screenSize.X / 2f + 269, 30 + y1);
			Bounds2 leftBounds = Bounds2.WithSize(leftPos, new Vector2(36f, 37f));
			Bounds2 rightBounds = Bounds2.WithSize(rightPos, new Vector2(36f, 37f));
			if(leftBounds.Contains(InputManager.MousePos()))
				TextureRenderer.Render(Assets.textures.UI.checkbox_hover, Color.White.WithAlpha(0.7f), leftPos);
			else TextureRenderer.Render(Assets.textures.UI.checkbox, Color.White.WithAlpha(0.7f), leftPos);
			if(rightBounds.Contains(InputManager.MousePos()))
				TextureRenderer.Render(Assets.textures.UI.checkbox_hover, Color.White.WithAlpha(0.7f), rightPos);
			else TextureRenderer.Render(Assets.textures.UI.checkbox, Color.White.WithAlpha(0.7f), rightPos);
            TextureRenderer.Render(Assets.textures.instructions.track_retreat, leftPos);
            TextureRenderer.Render(Assets.textures.instructions.track_advance, rightPos);
            // show the currently displayed campaign
            TextureRenderer.RenderText(((patch_Campaign)(object)QuintessentialLoader.AllCampaigns[currentCampaign]).QuintTitle, new Vector2(InputManager.screenSize.X / 2f, 20 + y1), Assets.fonts.crimson_16_5, Color.LightGray, TextAlignment.Center,1,0.6f,float.MaxValue,float.MaxValue,0,new Color(), null, int.MaxValue, false, true);
            // reopen the menu if clicked
            var settings = QuintessentialCoreSettings.Instance.SwitcherSettings;
			bool keyLeft = settings.SwitchCampaignLeft.Pressed();
			bool keyRight = settings.SwitchCampaignRight.Pressed();

			if((leftBounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) || keyLeft){
				Assets.sounds.click_button.PlaySound(1f);
				var next = currentCampaign - 1;
				if(next < 0)
					next += QuintessentialLoader.AllCampaigns.Count;
				currentCampaign = next;
				Campaigns.opusMagnum = QuintessentialLoader.AllCampaigns[currentCampaign];
				Campaigns.campaigns[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
				UI.InstantCloseScreen();
				UI.OpenScreen(new PuzzleSelectScreen());
			}else if((rightBounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) || keyRight) {
				Assets.sounds.click_button.PlaySound(1f);
				currentCampaign = (currentCampaign + 1) % QuintessentialLoader.AllCampaigns.Count;
				Campaigns.opusMagnum = QuintessentialLoader.AllCampaigns[currentCampaign];
				Campaigns.campaigns[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
				UI.InstantCloseScreen();
				UI.OpenScreen(new PuzzleSelectScreen());
			}
		}
	}

	public static void ResetPosition(){
		currentCampaign = 0;
	}
}