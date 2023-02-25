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
		int innerWidth = (int)dim.Width - 2 * padding;
		int innerHeight = (int)dim.Height - 2 * padding;
		spriteBatch.Draw(texture.Value, dim.Position(), new Rectangle(0, 0, padding, padding), Color.White);
		spriteBatch.Draw(texture.Value, new Rectangle((int)dim.X + padding, (int)dim.Y, innerWidth, padding), new Rectangle(padding, 0, 1, padding), Color.White);
		spriteBatch.Draw(texture.Value, new Vector2(dim.X + padding + innerWidth, dim.Y), new Rectangle(padding + 1, 0, padding, padding), Color.White);
		spriteBatch.Draw(texture.Value, new Rectangle((int)dim.X, (int)dim.Y + padding, padding, innerHeight), new Rectangle(0, padding, padding, 1), Color.White);
		spriteBatch.Draw(texture.Value, new Rectangle((int)dim.X + padding, (int)dim.Y + padding, innerWidth, innerHeight), new Rectangle(padding, padding, 1, 1), Color.White);
		spriteBatch.Draw(texture.Value, new Rectangle((int)dim.X + padding + innerWidth, (int)dim.Y + padding, padding, innerHeight), new Rectangle(padding + 1, padding, padding, 1), Color.White);
		spriteBatch.Draw(texture.Value, new Vector2(dim.X, dim.Y + padding + innerHeight), new Rectangle(0, padding + 1, padding, padding), Color.White);
		spriteBatch.Draw(texture.Value, new Rectangle((int)dim.X + padding, (int)dim.Y + padding + innerHeight, innerWidth, padding), new Rectangle(padding, padding + 1, 1, padding), Color.White);
		spriteBatch.Draw(texture.Value, new Vector2(dim.X + padding + innerWidth, dim.Y + padding + innerHeight), new Rectangle(padding + 1, padding + 1, padding, padding), Color.White);

		UpdateText();

		string drawText = text;

		Color color = text.Length == 0 ? Color.DimGray : Color.Black;

		if (text.Length == 0)
			drawText = focused ? string.Empty : defaultText.Value;

		DynamicSpriteFont font = FontAssets.MouseText.Value;
		float scale = innerHeight / font.MeasureString(drawText).Y;

		spriteBatch.DrawString(font, drawText, new Vector2(dim.X + padding, dim.Y + padding), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		if (focused && cursorTimer < 30)
		{
			scale = innerHeight / font.MeasureString(defaultText.Value).Y;
			float drawCursor = font.MeasureString(drawText.Substring(0, cursorPosition)).X * scale;
			spriteBatch.DrawString(font, "|", new Vector2(dim.X + padding + drawCursor, dim.Y + padding), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}

	public bool KeyPressed(Keys key)
	{
		return Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);
	}
}
