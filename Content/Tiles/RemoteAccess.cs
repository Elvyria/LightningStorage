using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using MagicStorage.Content.Items;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class RemoteAccess : StorageAccess
{
	public override ModTileEntity GetTileEntity()
	{
		return ModContent.GetInstance<TERemoteAccess>();
	}

	public override int ItemType(int frameX, int frameY)
	{
		return ModContent.ItemType<Items.RemoteAccess>();
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override TEStorageHeart GetHeart(int i, int j)
	{
		TileEntity ent = TileEntity.ByPosition[new Point16(i, j)];
		return ((TERemoteAccess)ent).GetHeart();
	}

	public override bool RightClick(int i, int j)
	{
		Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
		if (item.type == ModContent.ItemType<Locator>())
		{
			if (Main.tile[i, j].TileFrameX % 36 == 18)
			{
				i--;
			}
			if (Main.tile[i, j].TileFrameX % 36 == 18)
			{
				j--;
			}
			TERemoteAccess ent = (TERemoteAccess)TileEntity.ByPosition[new Point16(i, j)];
			Locator locator = (Locator)item.ModItem;
			string message;
			if (ent.TryLocate(locator.location, out message))
			{
				item.TurnToAir();
			}
			if (Main.LocalPlayer.selectedItem == 58)
			{
				Main.mouseItem = item.Clone();
			}
			Main.NewText(message);
			return true;
		}
		else
		{
			return base.RightClick(i, j);
		}
	}
}
