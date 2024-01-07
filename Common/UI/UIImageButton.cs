using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

namespace LightningStorage.Common.UI;

public class UIScrollButton : UIElement
{
	private Asset<Texture2D>  texture;
	private Asset<Texture2D>? hoverTexture;

	public Color color = Color.White;

	public bool active;

	public float opacityActive = 1.0f;
	public float opacityInactive = 0.4f;

	public UIScrollButton(Asset<Texture2D> texture, bool active = true)
	{
		this.texture = texture;
		this.active = active;

		Width.Pixels = texture.Width();
		Height.Pixels = texture.Height();
	}

	public UIScrollButton(Asset<Texture2D> texture, Color color) : this(texture)
	{
		this.color = color;
	}

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
    }

    public override void LeftClick(UIMouseEvent evt)
    {
		if (active)
		{
			base.LeftClick(evt);

			SoundEngine.PlaySound(SoundID.MenuTick);
		}
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
		float opacity = active ? opacityActive : opacityInactive;

		CalculatedStyle dimensions = GetDimensions();
		spriteBatch.Draw(texture.Value, dimensions.Position(), color * opacity);

		if (hoverTexture != null && base.IsMouseHovering && active)
		{
			spriteBatch.Draw(hoverTexture.Value, dimensions.Position(), Color.White);
		}
    }
}
