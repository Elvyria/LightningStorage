using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.DataStructures;
using Terraria.ObjectData;

using MagicStorage.Common;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class StorageUnit : StorageComponent
{
	private Asset<Texture2D> glowTexture;

	public class StyleID
	{
		public const byte Default         = 0;
		public const byte Demonite        = 1;
		public const byte Crimtane        = 2;
		public const byte Hellstone       = 3;
		public const byte Hallowed        = 4;
		public const byte BlueChlorophyte = 5;
		public const byte Luminite        = 6;
		public const byte Terra           = 7;
		public const byte Tiny            = 8;
	}

	public List<int> styles = new()
	{
		ModContent.ItemType<Items.StorageUnit>(),
		ModContent.ItemType<Items.StorageUnitDemonite>(),
		ModContent.ItemType<Items.StorageUnitCrimtane>(),
		ModContent.ItemType<Items.StorageUnitHellstone>(),
		ModContent.ItemType<Items.StorageUnitHallowed>(),
		ModContent.ItemType<Items.StorageUnitBlueChlorophyte>(),
		ModContent.ItemType<Items.StorageUnitLuminite>(),
		ModContent.ItemType<Items.StorageUnitTerra>(),
		ModContent.ItemType<Items.StorageUnitTiny>(),
	};

	public override void Load()
	{
		base.Load();
		glowTexture = Mod.Assets.Request<Texture2D>("Content/Tiles/StorageUnit_Glow");
	}

	public override void ModifyObjectData()
	{
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleMultiplier = 6;
		TileObjectData.newTile.StyleWrapLimit = 6;
	}

	public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TEStorageUnit>();

	public override void MouseOver(int i, int j)
	{
		Main.LocalPlayer.noThrow = 2;
	}

	public override int ItemType(int frameX, int frameY)
	{
		int style = frameY / 36;

		if (style < 0 || style > styles.Count)
		{
			return styles[StyleID.Default];
		}

		return styles[style];
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (Main.tile[i, j].TileFrameX % 36 == 18)
		{
			i--;
		}
		if (Main.tile[i, j].TileFrameY % 36 == 18)
		{
			j--;
		}

		Point16 pos = new Point16(i, j);
		if (TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TEStorageUnit unitEntity)
		{
			fail = !unitEntity.IsEmpty;
		}
	}

	public override bool RightClick(int i, int j)
	{
		if (Main.tile[i, j].TileFrameX % 36 == 18)
		{
			i--;
		}
		if (Main.tile[i, j].TileFrameY % 36 == 18)
		{
			j--;
		}

		if (TryUpgrade(i, j))
		{
			return true;
		}

		TEStorageUnit storageUnit = (TEStorageUnit)TileEntity.ByPosition[new Point16(i, j)];
		Main.LocalPlayer.tileInteractionHappened = true;
		string activeString = storageUnit.active ? "Active" : "Inactive";
		string fullnessString = storageUnit.NumItems + " / " + storageUnit.Capacity + " Items";
		Main.NewText(activeString + ", " + fullnessString);

		return base.RightClick(i, j);
	}

	private bool TryUpgrade(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Item item = player.inventory[player.selectedItem];
		bool success = false;

		if (item.ModItem is IStorageUpgrade upgrade)
		{
			if (upgrade.CanUpgrade(i, j))
			{
				upgrade.Upgrade(i, j);
				success = true;
			}
		}

		if (success)
		{
			TEStorageUnit storageUnit = (TEStorageUnit)TileEntity.ByPosition[new Point16(i, j)];
			storageUnit.UpdateTileFrame();
			TEStorageHeart heart = storageUnit.GetHeart();
			if (heart != null)
			{
				heart.ResetCompactStage();
			}
			item.stack--;
			if (item.stack <= 0)
			{
				item.TurnToAir();
			}
			if (player.selectedItem == 58)
			{
				Main.mouseItem = item.Clone();
			}
		}

		return success;
	}

	public static void SetStyle(int i, int j, int style)
	{
		Main.tile[i, j].TileFrameY = (short)(36 * style);
		Main.tile[i, j + 1].TileFrameY = (short)(36 * style + 18);
		Main.tile[i + 1, j].TileFrameY = (short)(36 * style);
		Main.tile[i + 1, j + 1].TileFrameY = (short)(36 * style + 18);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Vector2 drawPos = zero + 16f * new Vector2(i, j) - Main.screenPosition;
		Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		Color lightColor = Lighting.GetColor(i, j, Color.White);
		Color color = Color.Lerp(Color.White, lightColor, 0.5f);
		spriteBatch.Draw(glowTexture.Value, drawPos, frame, color);
	}
}
