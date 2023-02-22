using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.DataStructures;
using Terraria.ObjectData;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class StorageUnit : StorageComponent
{
	private Asset<Texture2D> glowTexture;

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

	public override ModTileEntity GetTileEntity()
	{
		return ModContent.GetInstance<TEStorageUnit>();
	}

	public override void MouseOver(int i, int j)
	{
		Main.LocalPlayer.noThrow = 2;
	}

	public override int ItemType(int frameX, int frameY)
	{
		int style = frameY / 36;
		int type;
		switch (style)
		{
			case 1:
				type = ModContent.ItemType<Items.StorageUnitDemonite>();
				break;
			case 2:
				type = ModContent.ItemType<Items.StorageUnitCrimtane>();
				break;
			case 3:
				type = ModContent.ItemType<Items.StorageUnitHellstone>();
				break;
			case 4:
				type = ModContent.ItemType<Items.StorageUnitHallowed>();
				break;
			case 5:
				type = ModContent.ItemType<Items.StorageUnitBlueChlorophyte>();
				break;
			case 6:
				type = ModContent.ItemType<Items.StorageUnitLuminite>();
				break;
			case 7:
				type = ModContent.ItemType<Items.StorageUnitTerra>();
				break;
			case 8:
				type = ModContent.ItemType<Items.StorageUnitTiny>();
				break;
			default:
				type = ModContent.ItemType<Items.StorageUnit>();
				break;
		}
		return type;
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
		Main.player[Main.myPlayer].tileInteractionHappened = true;
		string activeString = storageUnit.active ? "Active" : "Inactive";
		string fullnessString = storageUnit.NumItems + " / " + storageUnit.Capacity + " Items";
		Main.NewText(activeString + ", " + fullnessString);
		return base.RightClick(i, j);
	}

	private bool TryUpgrade(int i, int j)
	{
		Player player = Main.player[Main.myPlayer];
		Item item = player.inventory[player.selectedItem];
		int style = Main.tile[i, j].TileFrameY / 36;
		bool success = false;
		if (style == 0 && item.type == ModContent.ItemType<Items.UpgradeDemonite>())
		{
			SetStyle(i, j, 1);
			success = true;
		}
		else if (style == 0 && item.type == ModContent.ItemType<Items.UpgradeCrimtane>())
		{
			SetStyle(i, j, 2);
			success = true;
		}
		else if ((style == 1 || style == 2) && item.type == ModContent.ItemType<Items.UpgradeHellstone>())
		{
			SetStyle(i, j, 3);
			success = true;
		}
		else if (style == 3 && item.type == ModContent.ItemType<Items.UpgradeHallowed>())
		{
			SetStyle(i, j, 4);
			success = true;
		}
		else if (style == 4 && item.type == ModContent.ItemType<Items.UpgradeBlueChlorophyte>())
		{
			SetStyle(i, j, 5);
			success = true;
		}
		else if (style == 5 && item.type == ModContent.ItemType<Items.UpgradeLuminite>())
		{
			SetStyle(i, j, 6);
			success = true;
		}
		else if (style == 6 && item.type == ModContent.ItemType<Items.UpgradeTerra>())
		{
			SetStyle(i, j, 7);
			success = true;
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
				item.SetDefaults(0);
			}
			if (player.selectedItem == 58)
			{
				Main.mouseItem = item.Clone();
			}
		}
		return success;
	}

	private void SetStyle(int i, int j, int style)
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
