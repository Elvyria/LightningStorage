using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.DataStructures;
using Terraria.ObjectData;

using LightningStorage.Common;
using LightningStorage.Content.TileEntities;

namespace LightningStorage.Content.Tiles;

public class StorageUnit : StorageComponent, IUpgradeable
{
	[AllowNull]
	private Asset<Texture2D> glowTexture;

	public static class IndicatorStyle
	{
		public const byte Empty    = 0;
		public const byte Filled   = 1;
		public const byte Full     = 2;
		public const byte Inactive = 3;
	}

	public static class StyleID
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

	public override ModTileEntity? GetTileEntity() => ModContent.GetInstance<TEStorageUnit>();

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
		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

		Point16 pos = new Point16(i, j);
		if (TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TEStorageUnit unitEntity)
		{
			fail = !unitEntity.IsEmpty;
		}
	}

	public override bool RightClick(int i, int j)
	{
		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

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

		if (item.ModItem is IStorageUpgrade upgrade)
		{
			if (upgrade.CanUpgrade(i, j))
			{
				upgrade.Upgrade(i, j);

				TEStorageUnit storageUnit = (TEStorageUnit)TileEntity.ByPosition[new Point16(i, j)];
				storageUnit.UpdateTileFrame();

				TEStorageHeart? heart = storageUnit.GetHeart();
				heart?.ResetCompactStage();

				if (--item.stack <= 0)
				{
					item.TurnToAir();
				}

				return true;
			}
		}

		return false;
	}

	public static void SetIndicator(int i, int j, short style)
	{
		style *= 36;

		Main.tile[i, j].TileFrameX = style;
		Main.tile[i, j + 1].TileFrameX = style;
		Main.tile[i + 1, j].TileFrameX = unchecked((short)(style + 18));
		Main.tile[i + 1, j + 1].TileFrameX = unchecked((short)(style + 18));
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
