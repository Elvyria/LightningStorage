using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common;
using MagicStorage.Content.Tiles;

using IndicatorStyle = MagicStorage.Content.Tiles.StorageUnit.IndicatorStyle;

namespace MagicStorage.Content.TileEntities;

public class TEStorageUnit : TEStorageComponent
{
	public bool active = true;
	private Point16 center;

	private List<Item> items = new List<Item>();

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

	public TEStorageHeart? GetHeart()
	{
		if (center != Point16.NegativeOne && ByPosition.ContainsKey(center) && ByPosition[center] is TEStorageCenter storageCenter)
		{
			return storageCenter.GetHeart();
		}

		return null;
	}

	public bool Link(Point16 pos)
	{
		bool changed = pos != center;
		center = pos;
		return changed;
	}

	public bool Unlink() => Link(Point16.NegativeOne);

	public bool IsEmpty  => items.Count == 0;
	public bool IsFull   => items.Count >= Capacity;

	public int  NumItems => items.Count;

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

	public bool HasSpaceFor(Item check) => !IsFull || HasSpaceInStackFor(check);

	public bool HasItem(Item item) => hasItem.Contains(new ItemData(item));

	public IEnumerable<Item> GetItems() => items;

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
		UpdateTileFrame();

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
		UpdateTileFrame();

		Item result = lookFor.Clone();
		result.stack = lookFor.stack - amount;

		return result;
	}

	public void UpdateTileFrame()
	{
		short style = IsEmpty ? IndicatorStyle.Empty : IndicatorStyle.Filled;
		style = IsFull ? IndicatorStyle.Full : style;

		if (!active) style += IndicatorStyle.Inactive;

		StorageUnit.SetIndicator(Position.X, Position.Y, style);
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
		UpdateTileFrame();

		return item;
	}

	public override void SaveData(TagCompound tag)
	{
		List<TagCompound> tagItems = items
			.Where(item => !item.IsAir && item.stack > 0)
			.Select(ItemIO.Save)
			.ToList();

		tag.Set("Active", active);
		tag.SetPoint16("Center", center);
		tag.Set("Items",  tagItems);
	}

	public override void LoadData(TagCompound tag)
	{
		active = tag.GetBool("Active");
		center = tag.GetPoint16("Center");

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
