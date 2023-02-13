using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MagicStorage.Components
{
	public class StorageConnector : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(CanPlace, -1, 0, true);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			ModTranslation text = CreateMapEntryName();
			text.SetDefault("Magic Storage");
			AddMapEntry(new Color(153, 107, 61), text);

			DustType = 7;
			ItemDrop = ModContent.ItemType<Items.StorageConnector>();
		}

		public int CanPlace(int i, int j, int type, int style, int direction, int alternative)
		{
			int count = 0;

			Point16 startSearch = new Point16(i, j);
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
				if (!explored.Contains(explore) && explore != StorageComponent.killTile)
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

		public static int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternative)
		{
			TEStorageComponent.SearchAndRefreshNetwork(new Point16(i, j));
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
			StorageComponent.killTile = new Point16(i, j);
			TEStorageComponent.SearchAndRefreshNetwork(StorageComponent.killTile);
			StorageComponent.killTile = new Point16(-1, -1);
		}
	}
}
