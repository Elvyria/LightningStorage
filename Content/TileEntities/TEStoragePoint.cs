using System.Collections.Generic;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public abstract class TEStoragePoint : TEStorageComponent
    {
        private Point16 center;

        public void ResetAndSearch()
        {
            Point16 oldCenter = center;
            center = new Point16(-1, -1);

            HashSet<Point16> explored = new HashSet<Point16>();
            explored.Add(Position);
            Queue<Point16> toExplore = new Queue<Point16>();
            foreach (Point16 point in AdjacentComponents())
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
                        center = explore;
                        break;
                    }
                    foreach (Point16 point in AdjacentComponents(explore))
                    {
                        toExplore.Enqueue(point);
                    }
                }
            }
        }

        public override void OnPlace()
        {
            ResetAndSearch();
        }

        public bool Link(Point16 pos)
        {
            bool changed = pos != center;
            center = pos;
            return changed;
        }

        public bool Unlink()
        {
            return Link(new Point16(-1, -1));
        }

        public TEStorageHeart GetHeart()
        {
            if (center != new Point16(-1, -1))
            {
                return ((TEStorageCenter)ByPosition[center]).GetHeart();
            }
            return null;
        }

        public static bool IsStoragePoint(Point16 point)
        {
            return ByPosition.ContainsKey(point) && ByPosition[point] is TEStoragePoint;
        }

        public override void SaveData(TagCompound tag)
        {
            TagCompound tagCenter = new TagCompound();
            tagCenter.Set("X", center.X);
            tagCenter.Set("Y", center.Y);

            tag.Set("Center", tagCenter);
        }

        public override void LoadData(TagCompound tag)
        {
            TagCompound tagCenter = tag.GetCompound("Center");
            center = new Point16(tagCenter.GetShort("X"), tagCenter.GetShort("Y"));
        }
    }
}
