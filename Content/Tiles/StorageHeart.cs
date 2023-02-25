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

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Item selectedItem = player.inventory[player.selectedItem];

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

		if (selectedItem.type == ModContent.ItemType<PortableAccess>())
		{
			PortableAccess accessItem = (PortableAccess) selectedItem.ModItem;
			accessItem.storage = new Point16(i, j);
			if (player.selectedItem == 58)
			{
				Main.mouseItem = selectedItem.Clone();
			}
			Main.NewText("Portable access successfully set to: X=" + i + ", Y=" + j);

			return true;
		}

		return base.RightClick(i, j);
	}
}
