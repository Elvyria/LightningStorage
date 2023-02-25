using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities;

public class TEStorageUnit : TEStorageComponent
{
	public bool active = true;
	private Point16 center;

	private List<Item> items = new List<Item>();
	private bool receiving = false;

	private HashSet<ItemData> hasSpaceInStack = new HashSet<ItemData>();
	private HashSet<ItemData> hasItem = new HashSet<ItemData>();

	public static int[] capacities = new[] { 40, 80, 80, 120, 160, 240, 320, 640, 4 };

	public int Capacity
	{
		get
		{
			int style = Main.tile[Position.X, Position.Y].TileFrameY / 36;

			if (style < 0 || style >= capacities.Length)
			{
				return 0;
			}

			return capacities[style];
		}
	}

	public TEStorageHeart GetHeart()
	{
		if (center != Point16.NegativeOne && ByPosition.ContainsKey(center) && ByPosition[center] is TEStorageCenter)
		{
			return ((TEStorageCenter)ByPosition[center]).GetHeart();
		}
		return null;
	}

	public bool Link(Point16 pos)
	{
		bool changed = pos != center;
		center = pos;
		return changed;
	}

	public bool Unlink()
	{
		return Link(Point16.NegativeOne);
	}

	public bool IsFull
	{
		get => items.Count >= Capacity;
	}

	public bool IsEmpty
	{
		get => items.Count == 0;
	}

	public int NumItems
	{
		get => items.Count;
	}

	public override bool IsTileValidForEntity(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.HasTile && tile.TileType == ModContent.TileType<StorageUnit>();
	}

	public bool HasSpaceInStackFor(Item check)
	{
		ItemData data = new ItemData(check);
		return hasSpaceInStack.Contains(data);
	}

	public bool HasSpaceFor(Item check)
	{
		return !IsFull || HasSpaceInStackFor(check);
	}

	public bool HasItem(Item item)
	{
		return hasItem.Contains(new ItemData(item));
	}

	public IEnumerable<Item> GetItems()
	{
		return items;
	}

	public int Fill(Item deposit, int amount)
	{
		if (!HasSpaceInStackFor(deposit) || amount == 0)
		{
			return 0;
		}

		foreach (Item item in items)
		{
			if (ItemData.Matches(deposit, item) && item.stack < item.maxStack)
			{
				int available = item.maxStack - item.stack;

				if (available > amount)
				{
					item.stack += amount;
					return amount;
				}
				else
				{
					item.stack += available;
					hasSpaceInStack.Remove(new ItemData(item));
					return available;
				}
			}
		}

		return 0;
	}

	public int Deposit(Item deposit, int amount)
	{
		if (amount < 1) return 0;

		int filled = Fill(deposit, amount);

		if (filled == amount || (filled == 0 && IsFull))
		{
			return filled;
		}

		Item item = deposit.Clone();
		item.stack = amount - filled;
		item.newAndShiny = false;
		item.favorited = false;
		items.Add(item);

		RepairMetadata();

		return amount;
	}

	public Item Withdraw(Item lookFor)
	{
		int amount = lookFor.stack;

		for (int k = 0; k < items.Count; k++)
		{
			Item item = items[k];
			if (ItemData.Matches(lookFor, item))
			{
				if (item.stack > amount)
				{
					item.stack -= amount;
					amount = 0;
				}
				else
				{
					items.RemoveAt(k--);
					amount -= item.stack;
				}

				if (amount == 0)
				{
					break;
				}
			}
		}

		if (amount == lookFor.stack)
		{
			return new Item();
		}

		RepairMetadata();

		Item result = lookFor.Clone();
		result.stack = lookFor.stack - amount;

		return result;
	}

	public bool UpdateTileFrame()
	{
		Tile topLeft = Main.tile[Position.X, Position.Y];
		int oldFrame = topLeft.TileFrameX;
		int style;
		if (IsEmpty)
		{
			style = 0;
		}
		else if (IsFull)
		{
			style = 2;
		}
		else
		{
			style = 1;
		}
		if (!active)
		{
			style += 3;
		}
		style *= 36;
		topLeft.TileFrameX = (short)style;
		Main.tile[Position.X, Position.Y + 1].TileFrameX = (short)style;
		Main.tile[Position.X + 1, Position.Y].TileFrameX = (short)(style + 18);
		Main.tile[Position.X + 1, Position.Y + 1].TileFrameX = (short)(style + 18);

		return oldFrame != style;
	}

	internal static void SwapItems(TEStorageUnit unit1, TEStorageUnit unit2)
	{
		(unit1.items, unit2.items)
			= (unit2.items, unit1.items);

		(unit1.hasSpaceInStack, unit2.hasSpaceInStack)
			= (unit2.hasSpaceInStack, unit1.hasSpaceInStack);

		(unit1.hasItem, unit2.hasItem)
			= (unit2.hasItem, unit1.hasItem);

		unit1.UpdateTileFrame();
		unit2.UpdateTileFrame();
	}

	internal Item WithdrawStack()
	{
		Item item = items[items.Count - 1];
		items.RemoveAt(items.Count - 1);
		RepairMetadata();
		return item;
	}

	public override void SaveData(TagCompound tag)
	{
		TagCompound tagCenter = new TagCompound()
		{
			{ "X", center.X },
			{ "Y", center.Y }
		};

		List<TagCompound> tagItems = items
			.Where(item => !item.IsAir && item.stack > 0)
			.Select(ItemIO.Save)
			.ToList<TagCompound>();

		tag.Set("Active", active);
		tag.Set("Center", tagCenter);
		tag.Set("Items",  tagItems);
	}

	public override void LoadData(TagCompound tag)
	{
		active = tag.GetBool("Active");

		TagCompound tagCenter = tag.GetCompound("Center");
		center = new Point16(tagCenter.GetShort("X"), tagCenter.GetShort("Y"));

		ClearItems();
		items.Capacity = Capacity;
		foreach (TagCompound tagItem in tag.GetList<TagCompound>("Items"))
		{
			Item item = ItemIO.Load(tagItem);
			if (!item.IsAir && item.stack > 0) {
				items.Add(item);
				ItemData data = new ItemData(item);
				if (item.stack < item.maxStack)
				{
					hasSpaceInStack.Add(data);
				}
				hasItem.Add(data);
			}
		}
	}

	private void ClearItems()
	{
		items.Clear();
		hasSpaceInStack.Clear();
		hasItem.Clear();
	}

	private void RepairMetadata()
	{
		hasSpaceInStack.Clear();
		hasItem.Clear();

		foreach (Item item in items)
		{
			ItemData data = new ItemData(item);
			if (item.stack < item.maxStack)
			{
				hasSpaceInStack.Add(data);
			}
			hasItem.Add(data);
		}
	}
}
