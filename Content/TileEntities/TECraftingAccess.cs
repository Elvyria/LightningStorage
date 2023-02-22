using System.Linq;

using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public class TECraftingAccess : TEStorageComponent
    {
        public Item[] stations = new Item[10];

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

        public void TryDepositStation(Item item)
        {
			if (Array.Exists(stations, s => s.type == item.type))
				return;

            for (int k = 0; k < stations.Length; k++)
            {
                if (stations[k].IsAir)
                {
                    stations[k] = item.Clone();
                    stations[k].stack = 1;
                    item.stack--;
                    if (item.stack <= 0)
                    {
                        item.SetDefaults(0);
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
            else if (!item.IsAir && stations[slot].IsAir)
            {
                stations[slot] = item.Clone();
                stations[slot].stack = 1;
                item.stack--;
                if (item.stack <= 0)
                {
                    item.SetDefaults(0);
                }
                return item;
            }

            return item;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["Stations"] = stations.Select(item => ItemIO.Save(item)).ToList();
        }

        public override void LoadData(TagCompound tag)
        {
            IList<TagCompound> stations = tag.GetList<TagCompound>("Stations");
            if (stations != null && stations.Any())
            {
                this.stations = stations.Select(item => ItemIO.Load(item)).ToArray();
            }
        }
    }
}
