using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;

using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;

namespace MagicStorage.Common.UI;

public class UISlotZone : UIElement
{
	public static readonly Item Air = new Item();

	public int mouseSlot = -1;
	public int padding = 4;

	private int columns = 1;
	private int rows = 1;

	private Func<int, Item> getItem;
	private Func<int, Color> getColor;

	private float scale;
	private float slotHeight;
	private float slotWidth;

	private UIScrollbar? scrollbar;

	public UISlotZone(Func<int, Item> getItem, float scale)
	{
		this.getItem = getItem;
		getColor = (_) => Color.White;

		this.scale = scale;
		slotHeight = TextureAssets.InventoryBack.Height() * scale;
		slotWidth = TextureAssets.InventoryBack.Width() * scale;

		Width.Set(slotWidth, 0);
		Height.Set(slotHeight, 0);
	}

	public UISlotZone(Func<int, Item> getItem, Func<int, Color> getColor, float scale)
	{
		this.getItem = getItem;
		this.getColor = getColor;

		this.scale = scale;
		slotHeight = TextureAssets.InventoryBack.Height() * scale;
		slotWidth = TextureAssets.InventoryBack.Width() * scale;
	}

	public void SetScale(float scale)
	{
		this.scale = scale;
		slotHeight = TextureAssets.InventoryBack.Height() * scale;
		slotWidth = TextureAssets.InventoryBack.Width() * scale;
	}

	public void SetDimensions(int columns, int rows)
	{
		if (this.rows != rows)
		{
			this.rows = rows;
			Height.Set(slotHeight * rows + (rows - 2) * padding, 0);
			if (scrollbar != null)
				scrollbar.Height = Height;
		}

		if (this.columns != columns)
		{
			this.columns = columns;
			Width.Set(slotWidth * columns + (columns - 2) * padding, 0);
			if (scrollbar != null)
				scrollbar.Left.Set(Width.Pixels + scrollbar.Left.Pixels, 0);
		}
	}

	public void SetScrollbar(UIScrollbar scrollbar)
	{
		this.scrollbar = scrollbar;

		scrollbar.Top.Set(Top.Pixels + padding, Top.Percent);
		scrollbar.Left.Set(Width.Pixels + scrollbar.Left.Pixels, 0);
		scrollbar.Height = Height;
	}

	public void UpdateScrollBar(int rows)
	{
		scrollbar?.SetView(this.rows, rows);
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);
		if (scrollbar != null)
		{
			scrollbar.ViewPosition -= Math.Sign(evt.ScrollWheelValue);
		}
	}

	public int MouseSlot()
	{
		if (!IsMouseHovering) return -1;

		float x = Main.mouseX - GetDimensions().X;
		float y = Main.mouseY - GetDimensions().Y;

		float slotWidth = this.slotWidth + padding;
		float slotHeight = this.slotHeight + padding;

		int column = (int)(x / slotWidth);
		int row = (int)(y / slotHeight);

		// Padding Check
		if ((column + 1) * slotWidth - padding < x + padding || (row + 1) * slotHeight - padding < y + padding)
		{
			return -1;
		}

		int increment = scrollbar == null ? 0 : (int)scrollbar.ViewPosition;

		return increment * columns + row * columns + column;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		Vector2 origin = GetDimensions().Position();

		int increment = scrollbar == null ? 0 : (int)scrollbar.ViewPosition * columns;
		int length = columns * rows;

		for (int k = 0; k < length; k++)
		{
			int slot = k + increment;
			Vector2 drawPos = origin + new Vector2((slotWidth + padding) * (k % columns), (slotHeight + padding) * (k / columns));
			DrawSlot(spriteBatch, getItem(slot), getColor(slot), drawPos);
		}
	}

	private void DrawSlot(SpriteBatch spriteBatch, Item item, Color slotColor, Vector2 drawPos)
	{
		// Draw item slot background
		spriteBatch.Draw(TextureAssets.InventoryBack9.Value, drawPos, null, slotColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

		if (!item.IsAir)
		{
			float lightScale = 1f;
			Color color = Color.White;
			ItemSlot.GetItemLight(ref color, ref lightScale, item);

			Main.instance.LoadItem(item.type);
			Texture2D itemTexture = TextureAssets.Item[item.type].Value;
			Rectangle rectangle2 = Main.itemAnimations[item.type] == null ? itemTexture.Frame() : Main.itemAnimations[item.type].GetFrame(itemTexture);

			float itemScale = 1f;
			if (rectangle2.Width > 32 || rectangle2.Height > 32)
			{
				itemScale = 32f / Math.Max(rectangle2.Width, rectangle2.Height);
			}
			itemScale *= scale;

			Vector2 drawPosition = drawPos;
			drawPosition.X += (TextureAssets.InventoryBack9.Width() * scale - rectangle2.Width * itemScale) / 2;
			drawPosition.Y += (TextureAssets.InventoryBack9.Height() * scale - rectangle2.Height * itemScale) / 2;

			Vector2 origin = rectangle2.Size() * (lightScale / 2f - 0.5f);

			spriteBatch.Draw(itemTexture, drawPosition, rectangle2, item.GetAlpha(Color.White), 0, Vector2.Zero, itemScale, SpriteEffects.None, 0f);
			if (item.color != Color.Transparent)
			{
				spriteBatch.Draw(itemTexture, drawPosition, rectangle2, item.GetColor(Color.White), 0, Vector2.Zero, itemScale, SpriteEffects.None, 0f);
			}

			// Draw stack size
			if (item.stack > 1)
			{
				spriteBatch.DrawString(FontAssets.ItemStack.Value, item.stack.ToString(), new Vector2(drawPos.X + 10f * scale, drawPos.Y + 26f * scale), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}

	public void DrawText(SpriteBatch spriteBatch)
	{
		mouseSlot = MouseSlot();
		if (mouseSlot >= 0)
		{
			Item item = getItem(mouseSlot);
			if (!item.IsAir)
			{
				Main.hoverItemName = item.Name;
				Main.HoverItem = item.Clone();
			}
		}
	}
}
