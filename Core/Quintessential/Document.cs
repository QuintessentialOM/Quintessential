using Quintessential;
using Quintessential.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

public class Document {
    public Action<Vector2, string[]> CustomRender;
    public List<DocumentItem> items;
    public CampaignItem campaignItem;

    public void Render(Vector2 pos, string[] sections) {
        if (CustomRender != null) {
            CustomRender.Invoke(pos, sections);
            return;
        }

        foreach (var item in items) {
            if (item.texture != null) {
                Vector2 sizeOffset = item.GetScales(false) * item.texture.size.ToVector2();
                TextureRenderer.Render(item.texture, item.color, pos + item.offset + sizeOffset);
            } else {
                float verticalSizeOffset = item.GetScales(true).Y;
                if (verticalSizeOffset != 0f) verticalSizeOffset *= TextureRenderer.RenderText(sections[item.sectionIndex], pos + item.offset, item.font, item.color, item.GetTextAlignment(), item.spacing, 0.6f, item.lineWidth, float.MaxValue, 0, default, null, int.MaxValue, true, false).Height;
                TextureRenderer.RenderText(sections[item.sectionIndex], pos + item.offset + new Vector2(0, verticalSizeOffset), item.font, item.color, item.GetTextAlignment(), item.spacing, 0.6f, item.lineWidth, float.MaxValue, 0, default, null, int.MaxValue, false, true);
            }
        }
        if (InputManager.IsKeyPressed(SDL2.SDL.SDLKey.SDLK_F3)) {
            TryReparse();
        }
    }

    public void TryReparse() {
        try {
            if (campaignItem != null && campaignItem.campaign != null) {
                string path = ((patch_Campaign)(object)campaignItem.campaign).reloadPath;
                Campaign reloaded = DataLoader.LoadCampaign(path);
                var chFound = reloaded.chapters.First(ch => ch.campaignItems.Any(item => item.itemId == campaignItem.itemId));
                var itemFound = chFound.campaignItems.First(item => item.itemId == campaignItem.itemId);
                if (itemFound.vignette.HasValue()) {
                    var newDoc = ((patch_LocalizedVignette)(object)itemFound.vignette.GetValue()).Document;
                    if (newDoc != null) {
                        newDoc.campaignItem = campaignItem;
                        campaignItem.vignette = itemFound.vignette.GetValue();
                        if (GameLogic.instance.GetCurrentScreen() is DocumentScreen) {
                            UI.InstantCloseScreen();
                            UI.OpenScreen(new DocumentScreen(campaignItem.vignette.GetValue()));
                        }
                    }
                }
            }
        } catch (Exception e) { Logger.Log(e); }
    }
}

public class DocumentItem() {
    public Texture texture;
    public int sectionIndex;
    public Vector2 offset;
    public Color color;
    public LocalizedFont font;
    public BoundsPosition alignment;
    public float spacing;
    public float lineWidth;


    public Vector2 GetScales(bool isText) {
        return alignment switch {
            BoundsPosition.TopLeft =>       new Vector2(0,     isText ? 0 : -1),
            BoundsPosition.TopEdge =>       new Vector2(-0.5f, isText ? 0 : -1),
            BoundsPosition.TopRight =>      new Vector2(-1,    isText ? 0 : -1),
            BoundsPosition.LeftEdge =>      new Vector2(0,     isText ? 0.5f : -0.5f),
            BoundsPosition.Center =>        new Vector2(-0.5f, isText ? 0.5f : -0.5f),
            BoundsPosition.RightEdge =>     new Vector2(-1,    isText ? 0.5f : -0.5f),
            BoundsPosition.BottomLeft =>    new Vector2(0,     isText ? 1 : 0),
            BoundsPosition.BottomEdge =>    new Vector2(-0.5f, isText ? 1 : 0),
            BoundsPosition.BottomRight =>   new Vector2(-1,    isText ? 1 : 0),
            _ => Vector2.Zero,
        };
    }
    public TextAlignment GetTextAlignment() {
        return alignment switch {
            BoundsPosition.TopLeft =>       TextAlignment.Left,
            BoundsPosition.TopEdge =>       TextAlignment.Center,
            BoundsPosition.TopRight =>      TextAlignment.Right,
            BoundsPosition.LeftEdge =>      TextAlignment.Left,
            BoundsPosition.Center =>        TextAlignment.Center,
            BoundsPosition.RightEdge =>     TextAlignment.Right,
            BoundsPosition.BottomLeft =>    TextAlignment.Left,
            BoundsPosition.BottomEdge =>    TextAlignment.Center,
            BoundsPosition.BottomRight =>   TextAlignment.Right,
            _ => TextAlignment.Left,
        };
    }
}
