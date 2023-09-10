using Microsoft.Xna.Framework;

using Terraria.DataStructures;
using Terraria.ObjectData;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Tiles;

public class StorageConnector : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileNoAttach[Type] = true;
		Main.tileSolid[Type] = false;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.LavaDeath = false;

		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Hook_AfterPlacement, -1, 0, false);
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(153, 107, 61), CreateMapEntryName());

		DustType = 7;
	}

	public static int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternative)
	{
		TEStorageComponent.RefreshNetwork(new Point16(i, j));
		return 0;
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		int frameX = 0;
		int frameY = 0;
		if (WorldGen.InWorld(i - 1, j) && Main.tile[i - 1, j].HasTile && Main.tile[i - 1, j].TileType == Type)
		{
			frameX += 18;
		}
		if (WorldGen.InWorld(i + 1, j) && Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].TileType == Type)
		{
			frameX += 36;
		}
		if (WorldGen.InWorld(i, j - 1) && Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].TileType == Type)
		{
			frameY += 18;
		}
		if (WorldGen.InWorld(i, j + 1) && Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == Type)
		{
			frameY += 36;
		}
		Main.tile[i, j].TileFrameX = (short)frameX;
		Main.tile[i, j].TileFrameY = (short)frameY;
		return false;
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (fail || effectOnly)
		{
			return;
		}
		TEStorageComponent.RefreshNetwork(new Point16(i, j));
	}
}
