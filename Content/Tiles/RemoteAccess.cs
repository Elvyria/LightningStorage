using Microsoft.Xna.Framework;

using Terraria.DataStructures;

using MagicStorage.Common.Players;
using MagicStorage.Content.Items;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class RemoteAccess : StorageAccess
{
	public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TERemoteAccess>();

	public override int ItemType(int frameX, int frameY) => ModContent.ItemType<Items.RemoteAccess>();

	public override TEStorageHeart? GetHeart(int i, int j)
	{
		TileEntity ent = TileEntity.ByPosition[new Point16(i, j)];
		return ((TERemoteAccess)ent).GetHeart();
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		r = 0.15f * (MathF.Sin((float)Main.timeForVisualEffects * 0.01f - MathHelper.PiOver2) + 3f);
	}

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Item selectedItem = player.inventory[player.selectedItem];

		if (selectedItem.type == ModContent.ItemType<Locator>())
		{
			return false;
		}

		if (ItemSlot.ShiftInUse)
		{
			(i, j) = Main.tile[i, j].FrameOrigin(i, j);
			Point16 pos = new Point16(i, j);
			TERemoteAccess access = (TERemoteAccess) TileEntity.ByPosition[pos];
			access.Reset();

			if (pos == StoragePlayer.LocalPlayer.ViewingStorage())
			{
				StoragePlayer.LocalPlayer.CloseStorage();
			}

			return false;
		}

		return base.RightClick(i, j);
	}
}
