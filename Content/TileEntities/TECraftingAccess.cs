using System.Linq;

using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities;

public class TECraftingAccess : TEStorageComponent
{
    public Item[] stations = new Item[capacities[0]];

	public static int[] capacities = new[] { 10, 12, 12, 14, 16, 18, 19, 20 };

    public TECraftingAccess()
    {
        for (int k = 0; k < stations.Length; k++)
        {
            stations[k] = new Item();
        }
    }

    public override bool IsTileValidForEntity(int i, int j)
    {
		Tile tile = Main.tile[i, j];
        return tile.HasTile && tile.TileType == ModContent.TileType<CraftingAccess>();
    }

    public void DepositStation(Item item)
    {
		if (Array.Exists(stations, s => s.type == item.type)) return;

        for (int k = 0; k < stations.Length; k++)
        {
            if (stations[k].IsAir)
            {
                stations[k] = item.Clone();
                stations[k].stack = 1;
                item.stack--;
                if (item.stack <= 0)
                {
                    item.TurnToAir();
                }
                return;
            }
        }
    }

    public Item WithdrawStation(int slot)
    {
        if (!stations[slot].IsAir)
        {
            Item item = stations[slot];
            stations[slot] = new Item();

            return item;
        }

        return new Item();
    }

    public Item SwapStations(Item item, int slot)
    {
        if (!item.IsAir)
        {
            for (int k = 0; k < stations.Length; k++)
            {
                if (k != slot && stations[k].type == item.type)
                {
                    return item;
                }
            }
        }

        if ((item.IsAir || item.stack == 1) && !stations[slot].IsAir)
        {
            Item temp = item;
            item = stations[slot];
            stations[slot] = temp;
            return item;
        }

        if (!item.IsAir && stations[slot].IsAir)
        {
            stations[slot] = item.Clone();
            stations[slot].stack = 1;
            item.stack--;
            if (item.stack <= 0)
            {
                item.TurnToAir();
            }
            return item;
        }

        return item;
    }

    public override void OnPlace()
    {
        base.OnPlace();

		Resize();
    }

	public void Resize()
	{
		int style = Main.tile[Position.X, Position.Y].TileFrameY / 36;

		if (style >= 0 && style < capacities.Length)
		{
			Array.Resize(ref stations, capacities[style]);

			for (int k = 0; k < stations.Length; k++)
			{
				if (stations[k] == null)
				{
					stations[k] = new Item();
				}
			}
		}
	}

    public override void SaveData(TagCompound tag)
    {
        tag["Stations"] = stations.Select(ItemIO.Save).ToList();
    }

    public override void LoadData(TagCompound tag)
    {
        IList<TagCompound> stations = tag.GetList<TagCompound>("Stations");
        if (stations != null && stations.Any())
        {
            this.stations = stations.Select(ItemIO.Load).ToArray();
        }
    }
}
