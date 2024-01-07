using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using Terraria.ObjectData;
using LightningStorage.Content.TileEntities;

namespace LightningStorage.Content.Tiles;

public abstract class StorageComponent : ModTile
{
	public override string HighlightTexture => typeof(StorageComponent)?.FullName?.Replace('.', '/') + "_Highlight";

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
		ModTileEntity? tileEntity = GetTileEntity();

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

	public virtual ModTileEntity? GetTileEntity() => null;

	public virtual int ItemType(int frameX, int frameY) => ModContent.ItemType<Items.StorageComponent>();

	public override bool CanPlace(int i, int j)
	{
		return
			!( Framing.GetTileSafely(i, j - 1).HasTile
			|| Framing.GetTileSafely(i, j).HasTile
			|| Framing.GetTileSafely(i + 1, j - 1).HasTile
			|| Framing.GetTileSafely(i + 1, j).HasTile);
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ItemType(frameX, frameY));
		ModTileEntity? tileEntity = GetTileEntity();
		if (tileEntity != null)
		{
			tileEntity.Kill(i, j);
		}
		else
		{
			TEStorageComponent.RefreshNetwork(new Point16(i, j));
		}
	}

	public static void SetStyle(int i, int j, short style)
	{
		style *= 36;

		Main.tile[i, j].TileFrameY = style;
		Main.tile[i, j + 1].TileFrameY = unchecked((short)(style + 18));
		Main.tile[i + 1, j].TileFrameY = style;
		Main.tile[i + 1, j + 1].TileFrameY = unchecked((short)(style + 18));
	}
}
