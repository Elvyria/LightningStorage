using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles
{
    public class StorageComponent : ModTile
    {
        public static Point16 killTile = new Point16(-1, -1);

        public override string HighlightTexture { get { return typeof(StorageComponent).FullName.Replace('.', '/') + "_Highlight"; } }

        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(CanPlace, -1, 0, true);

            ModifyObjectData();
            ModTileEntity tileEntity = GetTileEntity();
            if (tileEntity != null)
            {
                TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
            }
            else
            {
                TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(TEStorageComponent.Hook_AfterPlacement_NoEntity, -1, 0, false);
            }
            TileObjectData.addTile(Type);

            ModTranslation text = CreateMapEntryName();
            text.SetDefault("Magic Storage");
            AddMapEntry(new Color(153, 107, 61), text);
            DustType = 7;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
        }

        public virtual void ModifyObjectData()
        {
        }

        public virtual ModTileEntity GetTileEntity()
        {
            return null;
        }

        public virtual int ItemType(int frameX, int frameY)
        {
            return ModContent.ItemType<Items.StorageComponent>();
        }

        public int CanPlace(int i, int j, int type, int style, int direction, int alternative)
        {
            int count = 0;
            if (GetTileEntity() != null && GetTileEntity() is TEStorageCenter)
            {
                count++;
            }

            Point16 startSearch = new Point16(i - 1, j - 1);
            HashSet<Point16> explored = new HashSet<Point16>();
            explored.Add(startSearch);
            Queue<Point16> toExplore = new Queue<Point16>();
            foreach (Point16 point in TEStorageComponent.AdjacentComponents(startSearch))
            {
                toExplore.Enqueue(point);
            }

            while (toExplore.Count > 0)
            {
                Point16 explore = toExplore.Dequeue();
                if (!explored.Contains(explore) && explore != killTile)
                {
                    explored.Add(explore);
                    if (TEStorageCenter.IsStorageCenter(explore))
                    {
                        count++;
                        if (count >= 2)
                        {
                            return -1;
                        }
                    }
                    foreach (Point16 point in TEStorageComponent.AdjacentComponents(explore))
                    {
                        toExplore.Enqueue(point);
                    }
                }
            }
            return count;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ItemType(frameX, frameY));
            killTile = new Point16(i, j);
            ModTileEntity tileEntity = GetTileEntity();
            if (tileEntity != null)
            {
                tileEntity.Kill(i, j);
            }
            else
            {
                TEStorageComponent.SearchAndRefreshNetwork(killTile);
            }
            killTile = new Point16(-1, -1);
        }
    }
}
