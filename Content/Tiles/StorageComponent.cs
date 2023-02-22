using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using Terraria.ObjectData;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles
{
    public class StorageComponent : ModTile
    {
		public override string HighlightTexture { get { return typeof(StorageComponent).FullName.Replace('.', '/') + "_Highlight"; } }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorValidWalls = new int[1];
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.LavaDeath = false;

            ModifyObjectData();
            ModTileEntity tileEntity = GetTileEntity();

			TileObjectData.newTile.HookPostPlaceMyPlayer = tileEntity != null ?
				new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false) :
				new PlacementHook(TEStorageComponent.Hook_AfterPlacement_NoEntity, -1, 0, false);

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(153, 107, 61), CreateMapEntryName());
            DustType = 7;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
        }

        public virtual void ModifyObjectData() {}

        public virtual ModTileEntity GetTileEntity()
        {
            return null;
        }

        public virtual int ItemType(int frameX, int frameY)
        {
            return ModContent.ItemType<Items.StorageComponent>();
        }

        // public override bool CanPlace(int i, int j)
        // {
            // return false;
        // }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ItemType(frameX, frameY));
			ModTileEntity tileEntity = GetTileEntity();
			if (tileEntity != null)
			{
				tileEntity.Kill(i, j);
			}
			else
			{
				TEStorageComponent.RefreshNetwork(new Point16(i, j));
			}
        }
    }
}
