using System.Linq;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MagicStorage.Components
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

		public override bool ValidTile(Tile tile)
		{
			return tile.TileType == ModContent.TileType<CraftingAccess>()
				&& tile.TileFrameX == 0
				&& tile.TileFrameY == 0;
		}

		public void TryDepositStation(Item item)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			foreach (Item station in stations)
			{
				if (station.type == item.type)
				{
					return;
				}
			}

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
					NetHelper.SendTEUpdate(ID, Position);
					return;
				}
			}
		}

		public Item TryWithdrawStation(int slot)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return new Item();
			}

			if (!stations[slot].IsAir)
			{
				Item item = stations[slot];
				stations[slot] = new Item();
				NetHelper.SendTEUpdate(ID, Position);

				return item;
			}

			return new Item();
		}

		public Item SwapStations(Item item, int slot)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return new Item();
			}

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
				NetHelper.SendTEUpdate(ID, Position);
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
				NetHelper.SendTEUpdate(ID, Position);
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
			IList<TagCompound> listStations = tag.GetList<TagCompound>("Stations");
			if (listStations != null && listStations.Any())
			{
				stations = listStations.Select(item => ItemIO.Load(item)).ToArray();
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			foreach (Item item in stations)
			{
				ItemIO.Send(item, writer, true, false);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			for (int k = 0; k < stations.Length; k++)
			{
				stations[k] = ItemIO.Receive(reader, true, false);
			}
		}
	}
}
