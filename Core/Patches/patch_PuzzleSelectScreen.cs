using MonoMod;
using MonoMod.Utils;
using Quintessential;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_PuzzleSelectScreen {
    private static int currentCampaign = 0;
    [MonoModIgnore]
    private float uiFadeTimer;
    [MonoModIgnore]
    private CampaignChapter currentChapter;
    [MonoModIgnore]
    public extern void orig_RenderFrame(float deltaTime);

    bool needsReset;
	public void RenderFrame(float deltaTime) {
        needsReset = needsReset || currentChapter.campaign == Campaigns.deReMetallica;
        orig_RenderFrame(deltaTime);
        if (QuintessentialCoreSettings.Instance.EnableCustomCampaigns && QuintessentialLoader.AllCampaigns.Count > 1) {
			if (needsReset && currentChapter.campaign != Campaigns.deReMetallica && uiFadeTimer == 1) {
                needsReset = false;
                GameLogic.instance.saveData.SetDefaultChapter(int.Clamp(GameLogic.instance.saveData.GetDefaultChapter(), 0, Campaigns.opusMagnum.chapters.Length - 1));
            }
            var dyn = new DynamicData(typeof(PuzzleSelectScreen), this);
			float y1 = Utils.InterpolateCos(-220f, 0.0f, dyn.Get<float>("chapterUIFadeTimer"));
			// add campaign change buttons
			Vector2 leftPos = new(InputManager.screenSize.X / 2f - 305, 30 + y1);
			Vector2 rightPos = new(InputManager.screenSize.X / 2f + 269, 30 + y1);
			Bounds2 leftBounds = Bounds2.WithSize(leftPos, new Vector2(36f, 37f));
			Bounds2 rightBounds = Bounds2.WithSize(rightPos, new Vector2(36f, 37f));
			if(leftBounds.Contains(InputManager.MousePos()))
				TextureRenderer.Render(Assets.textures.UI.checkbox_hover, Color.White.WithAlpha(0.7f * uiFadeTimer), leftPos);
			else TextureRenderer.Render(Assets.textures.UI.checkbox, Color.White.WithAlpha(0.7f * uiFadeTimer), leftPos);
			if(rightBounds.Contains(InputManager.MousePos()))
				TextureRenderer.Render(Assets.textures.UI.checkbox_hover, Color.White.WithAlpha(0.7f * uiFadeTimer), rightPos);
			else TextureRenderer.Render(Assets.textures.UI.checkbox, Color.White.WithAlpha(0.7f * uiFadeTimer), rightPos);
            TextureRenderer.Render(Assets.textures.instructions.track_retreat, Color.White.WithAlpha(uiFadeTimer), leftPos);
            TextureRenderer.Render(Assets.textures.instructions.track_advance, Color.White.WithAlpha(uiFadeTimer), rightPos);
            // show the currently displayed campaign
            TextureRenderer.RenderText(((patch_Campaign)(object)QuintessentialLoader.AllCampaigns[currentCampaign]).QuintTitle, new Vector2(InputManager.screenSize.X / 2f, 20 + y1), Assets.fonts.crimson_16_5, Color.LightGray.WithAlpha(uiFadeTimer), TextAlignment.Center,1,0.6f,float.MaxValue,float.MaxValue,0,new Color(), null, int.MaxValue, false, true);
            // reopen the menu if clicked
            if (!needsReset) {
                var settings = QuintessentialCoreSettings.Instance.SwitcherSettings;
                bool keyLeft = settings.SwitchCampaignLeft.Pressed();
                bool keyRight = settings.SwitchCampaignRight.Pressed();
                if ((leftBounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) || keyLeft){
				    Assets.sounds.click_button.PlaySound(1f);
                    SetCampaign(currentCampaign - 1);
			    }else if((rightBounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) || keyRight) {
				    Assets.sounds.click_button.PlaySound(1f);
                    SetCampaign(currentCampaign + 1);
			    }
            }
		}
	}

    public static void SetCampaign(int newCampaignIndex) {
        currentCampaign = Utils.Modulo(newCampaignIndex, QuintessentialLoader.AllCampaigns.Count);
        Campaigns.opusMagnum = QuintessentialLoader.AllCampaigns[currentCampaign];
        Campaigns.campaigns[0] = QuintessentialLoader.AllCampaigns[currentCampaign];
        GameLogic.instance.saveData.SetDefaultChapter(int.Clamp(GameLogic.instance.saveData.GetDefaultChapter(), 0, Campaigns.opusMagnum.chapters.Length - 1));
        UI.InstantCloseScreen();
        UI.OpenScreen(new PuzzleSelectScreen());
    }
}