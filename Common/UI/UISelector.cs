using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.GameContent;

namespace LightningStorage.Common.UI;

public class UISelector : UIElement, ISwitchable
{

#nullable disable
	private Vector2 circleOrigin;
	private Texture2D circleBackground;
	private Texture2D circleSelected;

	private Asset<Texture2D>[] textures;

	private Vector2[] targetPositions;
	private Vector2[] positions;

	public bool Opening { get; private set; }
	public bool Closing { get; private set; }

	private const float defaultScale = 0.7f;

	float easeTimer;
	private float scale = defaultScale;
	private float opacity;

	private float closingOpacity;
	private float closingScale;
	private Vector2[] closingPositions;

	public int Selected { get; private set; }
	public bool Visible { get => opacity > 0; }

	private float circleRadius;
#nullable restore

	public UISelector(Asset<Texture2D>[] textures, Vector2[] positions)
	{
		this.textures = textures;
		this.targetPositions = positions;
	}

    public override void OnInitialize()
    {
		circleBackground = TextureAssets.WireUi[0].Value;
		circleSelected = TextureAssets.WireUi[1].Value;
		circleOrigin = new Vector2(circleBackground.Height, circleBackground.Width) / 2;
		circleRadius = MathF.Sqrt(MathF.Pow(circleOrigin.X, 2) + MathF.Pow(circleOrigin.Y, 2)) * defaultScale;
    }

    public override void OnActivate()
    {
		Selected = -1;
		easeTimer = 0;

		positions = new Vector2[targetPositions.Length];
    }

    public override void OnDeactivate()
    {
		Opening = false;
		Closing = false;

		opacity = 1f;
		scale = defaultScale;

		positions = null;
		closingPositions = null;
    }

	public void Open(bool animate = false)
	{
		easeTimer = 0;
		Closing = false;
		Opening = animate;
	}

	public void Close(bool animate = false)
	{
		easeTimer = 0;
		Opening = false;

		if (animate)
		{
			Closing = true;
			closingPositions = positions.Clone() as Vector2[];
			closingOpacity = opacity;
			closingScale = scale;
		}
	}

    public override void Update(GameTime gameTime)
    {
		if (Closing)
		{
			AnimateClosing();
			return;
		}

		if (Opening)
		{
			AnimateOpening();
		}

		if (!Visible) return;

		Vector2 mouse = Main.MouseScreen;
		Vector2 origin = GetDimensions().Position();

		int oldSelected = Selected;
		Selected = -1;

		for (int i = 0; i < positions.Length; i++)
		{
			Vector2 pos = origin + positions[i] * Main.GameZoomTarget;

			if (mouse.Distance(pos) <= circleRadius)
			{
				Selected = i;
			}
		}

		if (Selected != -1 && oldSelected != Selected && !Opening && !Closing)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
		}
    }

	private void AnimateOpening()
	{
		const float speed = 0.15f;
		scale = Animation.Ease(0f, defaultScale, speed, ref easeTimer);

		for (int i = 0; i < positions.Length; i++)
		{
			positions[i].X = Animation.Ease(0f, targetPositions[i].X, speed, easeTimer);
			positions[i].Y = Animation.Ease(0f, targetPositions[i].Y, speed, easeTimer);
		}

		opacity = Animation.Ease(0f, 1f, speed, easeTimer);

		if (opacity >= 0.99f)
		{
			Opening = false;

			scale = defaultScale;
			positions = targetPositions.Clone() as Vector2[];
			opacity = 1f;
		}
	}

	private void AnimateClosing()
	{
		const float speed = 0.15f;
		scale = Animation.Ease(closingScale, 0f, speed, ref easeTimer);

		for (int i = 0; i < positions.Length; i++)
		{
			positions[i].X = Animation.Ease(closingPositions[i].X, 0f, speed, easeTimer);
			positions[i].Y = Animation.Ease(closingPositions[i].Y, 0f, speed, easeTimer);
		}

		opacity = Animation.Ease(closingOpacity, 0f, speed, easeTimer);

		if (opacity <= 0.01f)
		{
			Closing = false;
			opacity = 0f;
		}
	}

    public override void Draw(SpriteBatch spriteBatch)
    {
		if (!Visible) return;

		Vector2 origin = GetDimensions().Position();

		for (int i = 0; i < positions.Length; i++)
		{
			Vector2 pos = origin + positions[i] * Main.GameZoomTarget;

			DrawCircle(spriteBatch, textures[i].Value, pos, Selected == i);
		}
    }

	private void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, Vector2 pos, bool selected)
	{
		Color color = Color.White * opacity;
		Texture2D circleTexture = selected ? circleSelected : circleBackground;

		spriteBatch.Draw(circleTexture, pos, null, color, 0f, circleOrigin, scale, SpriteEffects.None, 0f);

		Vector2 origin = new Vector2(texture.Height, texture.Width) / 2;
		spriteBatch.Draw(texture, pos, null, color, 0f, origin, 0.5f, SpriteEffects.None, 0f);
	}
}
