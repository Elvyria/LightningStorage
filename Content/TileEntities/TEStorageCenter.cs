using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public abstract class TEStorageCenter : TEStorageComponent
    {
        public List<Point16> storageUnits = new List<Point16>();

        public void ResetAndSearch()
        {
            List<Point16> oldStorageUnits = new List<Point16>(storageUnits);
            storageUnits.Clear();
            HashSet<Point16> hashStorageUnits = new HashSet<Point16>();
            HashSet<Point16> explored = new HashSet<Point16>();
            explored.Add(Position);
            Queue<Point16> toExplore = new Queue<Point16>();
            foreach (Point16 point in AdjacentComponents())
            {
                toExplore.Enqueue(point);
            }
            bool changed = false;

            while (toExplore.Count > 0)
            {
                Point16 explore = toExplore.Dequeue();
                if (!explored.Contains(explore) && explore != StorageComponent.killTile)
                {
                    explored.Add(explore);
                    if (ByPosition.ContainsKey(explore) && ByPosition[explore] is TEAbstractStorageUnit)
                    {
                        TEAbstractStorageUnit storageUnit = (TEAbstractStorageUnit)ByPosition[explore];
                        if (storageUnit.Link(Position))
                        {
                            changed = true;
                        }
                        storageUnits.Add(explore);
                        hashStorageUnits.Add(explore);
                    }
                    foreach (Point16 point in AdjacentComponents(explore))
                    {
                        toExplore.Enqueue(point);
                    }
                }
            }

            foreach (Point16 oldStorageUnit in oldStorageUnits)
            {
                if (!hashStorageUnits.Contains(oldStorageUnit))
                {
                    if (ByPosition.ContainsKey(oldStorageUnit) && ByPosition[oldStorageUnit] is TEAbstractStorageUnit)
                    {
                        TileEntity storageUnit = ByPosition[oldStorageUnit];
                        ((TEAbstractStorageUnit)storageUnit).Unlink();
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                TEStorageHeart heart = GetHeart();
                if (heart != null)
                {
                    heart.ResetCompactStage();
                }
            }
        }

        public override void OnPlace()
        {
            ResetAndSearch();
        }

        public override void OnKill()
        {
            foreach (Point16 storageUnit in storageUnits)
            {
                TEAbstractStorageUnit unit = (TEAbstractStorageUnit)ByPosition[storageUnit];
                unit.Unlink();
            }
        }

        public abstract TEStorageHeart GetHeart();

        public static bool IsStorageCenter(Point16 point)
        {
            return ByPosition.ContainsKey(point) && ByPosition[point] is TEStorageCenter;
        }

        public override void SaveData(TagCompound tag)
        {
            List<TagCompound> tagUnits = new List<TagCompound>();
            foreach (Point16 storageUnit in storageUnits)
            {
                TagCompound tagUnit = new TagCompound();
                tagUnit.Set("X", storageUnit.X);
                tagUnit.Set("Y", storageUnit.Y);
                tagUnits.Add(tagUnit);
            }

            tag.Set("StorageUnits", tagUnits);
        }

        public override void LoadData(TagCompound tag)
        {
            foreach (TagCompound tagUnit in tag.GetList<TagCompound>("StorageUnits"))
            {
                storageUnits.Add(new Point16(tagUnit.GetShort("X"), tagUnit.GetShort("Y")));
            }
        }
    }
}
