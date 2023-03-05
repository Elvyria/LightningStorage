using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class CraftingAccess : StorageAccess
{
	public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TECraftingAccess>();

	public override int ItemType(int frameX, int frameY) => ModContent.ItemType<Items.CraftingAccess>();

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

		return base.RightClick(i, j);
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

		Point16 pos = new Point16(i, j);
		if (TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TECraftingAccess access)
		{
			foreach (Item item in access.stations)
			{
				fail |= !item.IsAir;
			}
		}
	}
}
