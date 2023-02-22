using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles
{
    public class CraftingAccess : StorageAccess
    {
        public override ModTileEntity GetTileEntity()
        {
            return ModContent.GetInstance<TECraftingAccess>();
        }

        public override int ItemType(int frameX, int frameY)
        {
            return ModContent.ItemType<Items.CraftingAccess>();
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override TEStorageHeart GetHeart(int i, int j)
        {
            Point16 point = TEStorageComponent.FindStorageCenter(new Point16(i, j));
            if (point.X < 0 || point.Y < 0 || !TileEntity.ByPosition.ContainsKey(point))
            {
                return null;
            }
            TileEntity heart = TileEntity.ByPosition[point];
            if (heart is not TEStorageCenter)
            {
                return null;
            }
            return ((TEStorageCenter)heart).GetHeart();
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
            if (TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TECraftingAccess access)
            {
                foreach (Item item in access.stations)
                {
					fail |= !item.IsAir;
                }
            }
        }
    }
}
