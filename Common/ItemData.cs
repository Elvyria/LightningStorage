namespace MagicStorage.Common;

public struct ItemData
{
	public readonly int Type;
	public readonly int Prefix;

	public ItemData(int type, int prefix = 0)
	{
		Type = type;
		Prefix = prefix;
	}

	public ItemData(Item item)
	{
		Type = item.type;
		Prefix = item.prefix;
	}

	public override bool Equals(object? other)
	{
		if (other is ItemData data)
		{
			return Matches(this, data);
		}

		return false;
	}

	public override int GetHashCode()
	{
		return 100 * Type + Prefix;
	}

	public static bool Matches(Item item1, Item item2)
	{
		return item1.type == item2.type && item1.prefix == item2.prefix;
	}

	public static bool Matches(ItemData data1, ItemData data2)
	{
		return data1.Type == data2.Type && data1.Prefix == data2.Prefix;
	}

	public static int Compare(Item a, Item b)
	{
		int result = a.type - b.type;

		if (result == 0)
		{
			result = a.prefix - b.prefix;
		}

		return result;
	}
}
