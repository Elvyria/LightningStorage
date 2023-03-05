using System.Linq;

namespace MagicStorage.Common.Sorting;

public static class ItemSorter
{
	public static List<Item> SortAndFilter(IEnumerable<Item> items, IComparer<Item> sortMode, IFilter<Item> filterMode, string modFilter, string nameFilter)
	{
		List<Item> result;

		if (filterMode != FilterMode.All || modFilter.Length != 0 || nameFilter.Length != 0)
		{
			result = items
				.Where(filterMode.Passes)
				.Where(item => FilterName(item, modFilter, nameFilter))
				.ToList();
		}
		else result = items.ToList();

		if (sortMode != SortMode.Quantity)
		{
			result.Sort(sortMode);
			Compact(result);
		}
		else
		{
			result.Sort(SortMode.Id);
			Compact(result);
			result.Sort(SortMode.Quantity);
		}

		return result;
	}

	// Must be sorted
	public static void Compact(List<Item> items)
	{
		int len = items.Count;

		for (int i = 0; i < len - 1; i++)
		{
			Item item = items[i];
			Item nextItem = items[i + 1];
			int shift = 0;

			if (item.type == nextItem.type && item.prefix == nextItem.prefix)
			{
				item = item.Clone();
				items[i] = item;
				item.stack += nextItem.stack;
				shift += 1;

				for (int j = i + 2; j < len; j++)
				{
					nextItem = items[j];

					if (item.type == nextItem.type && item.prefix == nextItem.prefix)
					{
						item.stack += nextItem.stack;

						shift += 1;
					}
					else break;
				}

				items.RemoveRange(i + 1, shift);
				len -= shift;
			}
		}
	}

	public static bool FilterName(Item item, string mod, string name)
	{
		string modName = item.ModItem == null ? "Terraria" : item.ModItem.Mod.DisplayName;

		return item.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 && modName.IndexOf(mod, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
