using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LightningStorage.Content.TileEntities;

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
            if (!explored.Contains(explore))
            {
                explored.Add(explore);
                if (ByPosition.ContainsKey(explore) && ByPosition[explore] is TEStorageUnit storageUnit)
                {
					changed |= storageUnit.Link(Position);
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
                if (ByPosition.ContainsKey(oldStorageUnit) && ByPosition[oldStorageUnit] is TEStorageUnit storageUnit)
                {
                    storageUnit.Unlink();
                }
                changed = true;
            }
        }

        if (changed)
        {
            TEStorageHeart? heart = GetHeart();
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
            TEStorageUnit unit = (TEStorageUnit) TileEntity.ByPosition[storageUnit];
            unit.Unlink();
        }
    }

    public abstract TEStorageHeart? GetHeart();

    public override void SaveData(TagCompound tag)
    {
		List<TagCompound> tagUnits = storageUnits
			.Select(TagExtensions.BuildTag)
			.ToList();

        tag.Set("StorageUnits", tagUnits);
    }

    public override void LoadData(TagCompound tag)
    {
		storageUnits = tag
			.GetList<TagCompound>("StorageUnits")
			.Select(TagExtensions.GetPoint16)
			.ToList();
    }
}
