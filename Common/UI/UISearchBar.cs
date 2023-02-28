using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;
using ReLogic.Graphics;

using Terraria.Localization;
using Terraria.GameInput;
using Terraria.GameContent;

namespace MagicStorage.Common.UI;

public class UISearchBar : UIElement
{
	private bool focused = false;
	private bool Focused { get { return focused; } }

	private const int padding = 4;
	private LocalizedText defaultText = Language.GetText("Mods.MagicStorage.Common.Search");
	private string text = string.Empty;
	private int cursorPosition = 0;
	private int cursorTimer = 0;

	private Asset<Texture2D> texture;

	public string Text { get => text; }

	public UISearchBar()
	{
		SetPadding(padding);
		OnClick += Focus;
		OnRightClick += Clear;
	}

	public override void OnInitialize()
	{
		texture = MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SearchBar");
	}

	public UISearchBar(LocalizedText defaultText) : this()
	{
		this.defaultText = defaultText;
	}

	public void Focus()
	{
		if (!focused)
		{
			focused = true;
			Main.blockInput = true;
		}
	}

	public void Clear()
	{
		text = string.Empty;
		cursorPosition = 0;
		cursorTimer = 0;
	}

	public void Clear(UIEvent _event, UIElement _element)
	{
		Clear();
	}

	public void Focus(UIEvent _event, UIElement _element)
	{
		Focus();
	}

	public void ResetFocus()
	{
		if (focused)
		{
			focused = false;
			Main.blockInput = false;
		}
	}

	public override void OnDeactivate()
	{
		ResetFocus();
		Clear();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsMouseHovering && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released)
			ResetFocus();
	}

	private void UpdateText()
	{
		cursorTimer++;
		cursorTimer %= 70;

		if (focused)
		{
			PlayerInput.WritingText = true;
			Main.instance.HandleIME();

			string old = text;

			text = Main.GetInputText(old.Substring(0, cursorPosition)) + old.Substring(cursorPosition);

			if (!text.Equals(old) && cursorPosition != text.Length)
				cursorPosition = text.Length;

			if (Main.keyState.IsKeyDown(Keys.Delete) && text.Length > 0 && cursorPosition <= text.Length - 1)
				text = text.Remove(cursorPosition, 1);
			else if (KeyPressed(Keys.Left) && cursorPosition > 0)
				cursorPosition--;
			else if (KeyPressed(Keys.Right) && cursorPosition < text.Length)
				cursorPosition++;
			else if (KeyPressed(Keys.Home))
				cursorPosition = 0;
			else if (KeyPressed(Keys.End))
				cursorPosition = text.Length;
			else if (KeyPressed(Keys.Enter) || KeyPressed(Keys.Tab) || KeyPressed(Keys.Escape))
				ResetFocus();
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dim = GetDimensions();

		Vector2 pos = dim.Position();

		Texture2D texture = this.texture.Value;
		int halfWidth = texture.Width / 2;
		int halfHeight = texture.Height / 2;

		Rectangle mainRectangle = new Rectangle(halfHeight, halfWidth, 1, 1);

		spriteBatch.Draw(texture, pos, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
		spriteBatch.Draw(texture, pos + new Vector2(halfWidth, dim.Height), new Rectangle(halfWidth, texture.Height, -halfWidth, -halfHeight), Color.White);

		spriteBatch.Draw(texture, pos + new Vector2(dim.Width, halfHeight), new Rectangle(texture.Width, halfHeight, -halfWidth, -halfHeight), Color.White);
		spriteBatch.Draw(texture, pos + new Vector2(dim.Width, dim.Height), new Rectangle(texture.Width, texture.Height, -halfWidth, -halfHeight), Color.White);

		spriteBatch.Draw(texture, new Rectangle((int) dim.X + halfWidth, (int) dim.Y, (int) dim.Width - 2 * halfWidth + 1, (int) dim.Height), mainRectangle, Color.White);
		spriteBatch.Draw(texture, new Rectangle((int) dim.X, (int) dim.Y + halfHeight, (int) dim.Width + 1, (int) dim.Height - 2 * halfHeight), mainRectangle, Color.White);

		int innerHeight = (int)dim.Height - 2 * halfHeight;

		string drawText = text;

		Color color = focused || text.Length != 0 ? Color.Black : Color.DimGray;

		if (text.Length == 0)
			drawText = focused ? string.Empty : defaultText.Value;

		DynamicSpriteFont font = FontAssets.MouseText.Value;
		float scale = innerHeight / font.MeasureString(drawText).Y;

		spriteBatch.DrawString(font, drawText, new Vector2(dim.X + halfWidth, dim.Y + halfHeight), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		if (focused && cursorTimer < 30)
		{
			scale = innerHeight / font.MeasureString(defaultText.Value).Y;
			float drawCursor = font.MeasureString(drawText.Substring(0, cursorPosition)).X * scale;
			spriteBatch.DrawString(font, "|", new Vector2(dim.X + halfWidth + drawCursor, dim.Y + halfHeight), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}

	public bool KeyPressed(Keys key)
	{
		return Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);
	}
}
