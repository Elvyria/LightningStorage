using Microsoft.Xna.Framework;

using Terraria.DataStructures;
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
		Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
		if (item.type == ModContent.ItemType<Locator>())
		{
			(i, j) = Main.tile[i, j].FrameOrigin(i, j);

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
