using System.Linq;

using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using Terraria.ObjectData;

using LightningStorage.Common;
using LightningStorage.Content.TileEntities;
using LightningStorage.Common.Players;

namespace LightningStorage.Content.Tiles;

public class CraftingAccess : StorageAccess, IUpgradeable
{
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
	}

	public List<int> styles = new()
	{
		ModContent.ItemType<Items.CraftingAccess>(),
		ModContent.ItemType<Items.CraftingAccessDemonite>(),
		ModContent.ItemType<Items.CraftingAccessCrimtane>(),
		ModContent.ItemType<Items.CraftingAccessHellstone>(),
		ModContent.ItemType<Items.CraftingAccessHallowed>(),
		ModContent.ItemType<Items.CraftingAccessBlueChlorophyte>(),
		ModContent.ItemType<Items.CraftingAccessLuminite>(),
		ModContent.ItemType<Items.CraftingAccessTerra>(),
	};

	public override void ModifyObjectData()
	{
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleMultiplier = 6;
		TileObjectData.newTile.StyleWrapLimit = 6;
	}

	public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TECraftingAccess>();

	public override int ItemType(int frameX, int frameY)
	{
		int style = frameY / 36;

		if (style < 0 || style > styles.Count)
		{
			return styles[StyleID.Default];
		}

		return styles[style];
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		b = 0.15f * (MathF.Sin((float)Main.timeForVisualEffects * 0.01f - MathHelper.PiOver2) + 3f);
	}

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Item selectedItem = player.inventory[player.selectedItem];

		if (selectedItem.type == ModContent.ItemType<Items.PortableAccess>())
		{
			return false;
		}

		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

		if (TryUpgrade(i, j))
		{
			return true;
		}

		return base.RightClick(i, j);
	}

	private bool TryUpgrade(int i, int j)
	{
		Item item = StoragePlayer.HeldItem();

		if (item.ModItem is IStorageUpgrade upgrade)
		{
			if (upgrade.CanUpgrade(i, j))
			{
				upgrade.Upgrade(i, j);

				TECraftingAccess access = (TECraftingAccess) TileEntity.ByPosition[new Point16(i, j)];
				access.Resize();

				if (--item.stack <= 0)
				{
					item.TurnToAir();
				}

				return true;
			}
		}

		return false;
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

		Point16 pos = new Point16(i, j);
		if (TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TECraftingAccess access)
		{
			fail = access.stations.Any(item => !item.IsAir);
		}
	}

}
