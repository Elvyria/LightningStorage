using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using MagicStorage.Content.Items;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class StorageHeart : StorageAccess
{
	public override ModTileEntity GetTileEntity()
	{
		return ModContent.GetInstance<TEStorageHeart>();
	}

	public override int ItemType(int frameX, int frameY)
	{
		return ModContent.ItemType<Items.StorageHeart>();
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override TEStorageHeart GetHeart(int i, int j)
	{
		return (TEStorageHeart) TileEntity.ByPosition[new Point16(i, j)];
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		g = 0.15f * (MathF.Sin((float)Main.timeForVisualEffects * 0.01f - MathHelper.PiOver2) + 3f);
	}

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Item selectedItem = player.inventory[player.selectedItem];

		if (selectedItem.type == ModContent.ItemType<PortableAccess>())
		{
			return false;
		}

		if (Main.tile[i, j].TileFrameX % 36 == 18)
		{
			i--;
		}
		if (Main.tile[i, j].TileFrameY % 36 == 18)
		{
			j--;
		}

		if (selectedItem.type == ModContent.ItemType<Locator>())
		{
			Locator locator = (Locator)selectedItem.ModItem;
			locator.location = new Point16(i, j);
			if (player.selectedItem == 58)
			{
				Main.mouseItem = selectedItem.Clone();
			}
			Main.NewText("Locator successfully set to: X=" + i + ", Y=" + j);

			return true;
		}

		return base.RightClick(i, j);
	}
}
