using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagicStorage.Sorting
{
	public static class ItemSorter
	{
		public static List<Item> SortAndFilter(IEnumerable<Item> items, SortMode sortMode, FilterMode filterMode, string modFilter, string nameFilter)
		{
			List<Item> result;

			if (filterMode != FilterMode.All || modFilter.Length != 0 || nameFilter.Length != 0)
			{
				Func<Item, bool> filterItem = ItemFilter(filterMode);

				result = items.Where(item => filterItem(item) && FilterName(item, modFilter, nameFilter)).ToList();
			}
			else result = items.ToList();

			if (sortMode == SortMode.Quantity)
			{
				result.Sort(new SortID());
				Compress(result);
				result.Sort(new SortQuantity());
			}
			else
			{
				result.Sort(Sort(sortMode));
				Compress(result);
			}

			return result;
		}

		public static Recipe[] SortAndFilter(Recipe[] recipes, SortMode sortMode, FilterMode filterMode, string modFilter, string nameFilter)
		{
			Recipe[] result = recipes;

			if (filterMode != FilterMode.All || modFilter.Length != 0 || nameFilter.Length != 0)
			{
				Func<Item, bool> filterItem = ItemFilter(filterMode);

				result = Array.FindAll(recipes, recipe => !recipe.createItem.IsAir && filterItem(recipe.createItem) && FilterName(recipe.createItem, modFilter, nameFilter));
			}
			else result = Array.FindAll(recipes, recipe => !recipe.createItem.IsAir);

			if (sortMode != SortMode.Default)
			{
				IComparer<Item> comparer = Sort(sortMode);
				Array.Sort(result, (a, b) => comparer.Compare(a.createItem, b.createItem));
			}

			return result;
		}

		// Must be sorted
		public static void Compress(List<Item> items)
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

							shift +=1;
						}
						else break;
					}

					items.RemoveRange(i + 1, shift);
					len -= shift;
				}

			}
		}

		private static Func<Item, bool> ItemFilter(FilterMode mode)
		{
			switch (mode)
			{
				case FilterMode.Weapons:
					return FilterWeapon.Passes;
				case FilterMode.Tools:
					return FilterTool.Passes;
				case FilterMode.Equipment:
					return FilterEquipment.Passes;
				case FilterMode.Potions:
					return FilterPotion.Passes;
				case FilterMode.Placeables:
					return FilterPlaceable.Passes;
				case FilterMode.Misc:
					return FilterMisc.Passes;
			}

			return FilterAll.Passes;
		}

		private static IComparer<Item> Sort(SortMode mode)
		{
			switch (mode)
			{
				case SortMode.Id:
					return new SortID();
				case SortMode.Name:
					return new SortName();
				case SortMode.Quantity:
					return new SortQuantity();
			}

			return new SortDefault();
		}

		public class SortDefault : IComparer<Item>
		{
			int IComparer<Item>.Compare(Item a, Item b)
			{
				return Compare(a, b);
			}

			internal static int Compare(Item a, Item b)
			{
				int aRating = Rate(a);
				int bRating = Rate(b);

				int result = aRating.CompareTo(bRating);

				if (result == 0)
				{
					if (aRating == SortRating.Ammo)
						result = -a.ammo.CompareTo(b.ammo);
					else if (aRating == SortRating.WeaponRanged)
						result = a.useAmmo.CompareTo(b.useAmmo);
				}

				if (result == 0)
				{
					if (aRating <= SortRating.Ammo)
						result = -a.damage.CompareTo(b.damage);
					else if (aRating == SortRating.Pickaxe)
						result = -a.pick.CompareTo(b.pick);
					else if (aRating == SortRating.Axe)
						result = -a.axe.CompareTo(b.axe);
					else if (aRating == SortRating.Hammer)
						result = -a.hammer.CompareTo(b.hammer);
					else if (aRating == SortRating.PotionHealth)
						result = -a.healLife.CompareTo(b.healLife);
					else if (aRating == SortRating.PotionMana)
						result = -a.healMana.CompareTo(b.healMana);
				}

				if (result == 0)
					result = a.type.CompareTo(b.type);

				if (result == 0)
					result = -a.rare.CompareTo(b.rare);

				if (result == 0)
					result = -a.prefix.CompareTo(b.prefix);

				return result;
			}

			static int Rate(Item item)
			{
				int rating = SortRating.Other;

				if (Weapon(item))
				{
					if (item.CountsAsClass(DamageClass.Melee))
						rating = SortRating.WeaponMelee;
					else if (item.CountsAsClass(DamageClass.Ranged))
						rating = SortRating.WeaponRanged;
					else if (item.CountsAsClass(DamageClass.Magic))
						rating = SortRating.WeaponMagic;
					else if (item.CountsAsClass(DamageClass.Summon))
						rating = SortRating.WeaponSummon;
					else if (item.CountsAsClass(DamageClass.Throwing))
						rating = SortRating.WeaponThrown;
					else
						rating = SortRating.Weapon;
				}

				if (item.ammo > 0 && item.damage > 0)
					return SortRating.Ammo;
				else if (item.axe > 0 && item.pick > 0)
					return SortRating.Picksaw;
				else if (item.axe > 0 && item.hammer > 0)
					return SortRating.Hamaxe;
				else if (item.pick > 0)
					return SortRating.Pickaxe;
				else if (item.axe > 0)
					return SortRating.Axe;
				else if (item.hammer > 0)
					return SortRating.Hammer;
				else if (Armor(item))
					return SortRating.Armor;
				else if (item.vanity)
					return SortRating.Vanity;
				else if (item.accessory)
					return SortRating.Accessory;
				else if (Hook(item))
					return SortRating.Hook;
				else if (item.mountType != -1)
					return SortRating.Mount;
				else if (PotionHealth(item))
					return SortRating.PotionHealth;
				else if (PotionMana(item))
					return SortRating.PotionMana;
				else if (PotionRestoration(item))
					return SortRating.PotionRestoration;
				else if (item.dye > 0)
					return SortRating.Dye;
				else if (item.hairDye > 0)
					return SortRating.DyeHair;
				else if (item.createTile != -1)
					return SortRating.Placeable;
				else if (item.consumable)
					return SortRating.Consumable;

				return rating;
			}

			static bool Tool(Item item)
			{
				return item.axe > 0 || item.pick > 0 || item.hammer > 0;
			}

			static bool PotionHealth(Item item)
			{
				return item.healLife > 0 && item.healMana < 1 && item.consumable;
			}

			static bool PotionMana(Item item)
			{
				return item.healMana > 0 && item.healLife < 0 && item.consumable;
			}

			static bool PotionRestoration(Item item)
			{
				return item.healLife > 0 && item.healMana > 0 && item.consumable;
			}

			static bool Hook(Item item)
			{
				return Main.projHook[item.shoot];
			}

			static bool Armor(Item item)
			{
				return !item.vanity && (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0);
			}

			static bool Weapon(Item item)
			{
				return !item.accessory && item.ammo == 0 && !Tool(item);
			}
		}

		public class SortID : IComparer<Item>
		{
			int IComparer<Item>.Compare(Item a, Item b)
			{
				int result = a.type.CompareTo(b.type);

				if (result == 0)
					return a.prefix.CompareTo(b.prefix);

				return result;
			}
		}

		public class SortType : IComparer<Item>
		{
			int IComparer<Item>.Compare(Item a, Item b)
			{
				return a.type.CompareTo(b.type);
			}
		}

		public class SortName : IComparer<Item>
		{
			int IComparer<Item>.Compare(Item a, Item b)
			{
				return a.Name.CompareTo(b.Name);
			}
		}

		public class SortQuantity : IComparer<Item>
		{
			int IComparer<Item>.Compare(Item a, Item b)
			{
				int result = -a.stack.CompareTo(b.stack);

				return result;
			}
		}

		public static bool FilterName(Item item, string mod, string name)
		{
			string modName = item.ModItem == null ? "Terraria" : item.ModItem.Mod.DisplayName;

			return item.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 && modName.IndexOf(mod, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
