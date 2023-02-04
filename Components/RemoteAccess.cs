using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Terraria;

using MagicStorage.Items;

namespace MagicStorage.Components
{
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
			Player player = Main.player[Main.myPlayer];
			Item item = player.inventory[player.selectedItem];
			if (item.type == ModContent.ItemType<Locator>() || item.type == ModContent.ItemType<LocatorDisk>())
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
					if (item.type == ModContent.ItemType<Items.LocatorDisk>())
					{
						locator.location = new Point16(-1, -1);
					}
					else
					{
						item.SetDefaults(0);
					}
				}
				if (player.selectedItem == 58)
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
}
