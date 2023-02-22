using Terraria.DataStructures;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public abstract class TEStorageComponent : ModTileEntity
    {
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int id = Place(i, j - 1);
            ((TEStorageComponent)ByID[id]).OnPlace();
            return id;
        }

        public static int Hook_AfterPlacement_NoEntity(int i, int j, int type, int style, int direction, int alternate)
        {
            RefreshNetwork(new Point16(i, j - 1));
            return 0;
        }

        public virtual void OnPlace()
        {
            RefreshNetwork(Position);
        }

        public override void OnKill()
        {
            RefreshNetwork(Position);
        }

        private static readonly Point16[] checkNeighbors = new Point16[]
        {
            new Point16(-1, 0),
            new Point16(0, -1),
            new Point16(1, -1),
            new Point16(2, 0),
            new Point16(2, 1),
            new Point16(1, 2),
            new Point16(0, 2),
            new Point16(-1, 1)
        };

        private static readonly Point16[] checkNeighbors1x1 = new Point16[]
        {
            new Point16(-1, 0),
            new Point16(0, -1),
            new Point16(1, 0),
            new Point16(0, 1)
        };

        public IEnumerable<Point16> AdjacentComponents()
        {
            return AdjacentComponents(Position);
        }

        public static IEnumerable<Point16> AdjacentComponents(Point16 point)
        {
            List<Point16> points = new List<Point16>(4);
            bool isConnector = Main.tile[point.X, point.Y].TileType == ModContent.TileType<StorageConnector>();
            foreach (Point16 add in isConnector ? checkNeighbors1x1 : checkNeighbors)
            {
                int checkX = point.X + add.X;
                int checkY = point.Y + add.Y;
                Tile tile = Main.tile[checkX, checkY];
                if (!tile.HasTile)
                {
                    continue;
                }

                if (tile.TileType == ModContent.TileType<StorageConnector>())
                {
					Point16 check = new Point16(checkX, checkY);
                    if (!points.Contains(check))
                    {
                        points.Add(check);
                    }

					continue;
                }

                if (TileLoader.GetTile(tile.TileType) is StorageComponent)
                {
                    if (tile.TileFrameX % 36 == 18)
                    {
						checkX--;
                    }
					
                    if (tile.TileFrameY % 36 == 18)
                    {
						checkY--;
                    }

					Point16 check = new Point16(checkX, checkY);
                    if (!points.Contains(check))
                    {
                        points.Add(check);
                    }
                }
            }
            return points;
        }

        public static Point16 FindStorageCenter(Point16 startSearch)
        {
            HashSet<Point16> explored = new HashSet<Point16>(8){ startSearch };
            Queue<Point16> toExplore = new Queue<Point16>(8);

            foreach (Point16 point in AdjacentComponents(startSearch))
            {
                toExplore.Enqueue(point);
            }

            while (toExplore.Count > 0)
            {
                Point16 explore = toExplore.Dequeue();
                if (!explored.Contains(explore))
                {
                    explored.Add(explore);

                    if (TileEntity.ByPosition.ContainsKey(explore) && TileEntity.ByPosition[explore] is TEStorageCenter)
                    {
                        return explore;
                    }

                    foreach (Point16 point in AdjacentComponents(explore))
                    {
                        toExplore.Enqueue(point);
                    }
                }
            }

            return Point16.NegativeOne;
        }

        public static void RefreshNetwork(Point16 position)
        {
            Point16 pos = FindStorageCenter(position);
            if (pos != Point16.NegativeOne)
            {
                TEStorageCenter center = ((TEStorageCenter) TileEntity.ByPosition[pos]);
                center.ResetAndSearch();
            }
        }
    }
}
