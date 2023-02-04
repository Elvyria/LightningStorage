using System;
using Terraria;
using Terraria.ModLoader;

namespace MagicStorage.Sorting
{
	public class FilterAll
	{
		public static bool Passes(Item item)
		{
			return true;
		}
	}

	public class FilterMelee
	{
		public static bool Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Melee) && item.pick == 0 && item.axe == 0 && item.hammer == 0;
		}
	}

	public class FilterRanged
	{
		public static bool Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Ranged);
		}
	}

	public class FilterMagic
	{
		public static bool Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Magic);
		}
	}

	public class FilterSummon
	{
		public static bool Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Summon);
		}
	}

	public class FilterThrown
	{
		public static bool Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Throwing);
		}
	}

	public class FilterOtherWeapon
	{
		public static bool Passes(Item item)
		{
			return !item.CountsAsClass(DamageClass.Melee) && !item.CountsAsClass(DamageClass.Ranged) && !item.CountsAsClass(DamageClass.Magic) && !item.CountsAsClass(DamageClass.Summon) && !item.CountsAsClass(DamageClass.Throwing) && item.damage > 0;
		}
	}

	public class FilterWeapon
	{
		public static bool Passes(Item item)
		{
			return item.damage > 0 && !item.accessory && item.pick == 0 && item.axe == 0 && item.hammer == 0;
		}
	}

	public class FilterPickaxe
	{
		public static bool Passes(Item item)
		{
			return item.pick > 0;
		}
	}

	public class FilterAxe
	{
		public static bool Passes(Item item)
		{
			return item.axe > 0;
		}
	}

	public class FilterHammer
	{
		public static bool Passes(Item item)
		{
			return item.hammer > 0;
		}
	}

	public class FilterTool
	{
		public static bool Passes(Item item)
		{
			return item.pick > 0 || item.axe > 0 || item.hammer > 0;
		}
	}

	public class FilterEquipment
	{
		public static bool Passes(Item item)
		{
			return item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0 || item.accessory || Main.projHook[item.shoot] || item.mountType >= 0 || (item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType]));
		}
	}

	public class FilterPotion
	{
		public static bool Passes(Item item)
		{
			return item.consumable && (item.healLife > 0 || item.healMana > 0 || item.buffType > 0);
		}
	}

	public class FilterPlaceable
	{
		public static bool Passes(Item item)
		{
			return item.createTile >= 0 || item.createWall > 0;
		}
	}

	public class FilterMisc
	{
		private static Func<Item, bool>[] blacklist = new Func<Item, bool>[] {
			FilterWeapon.Passes,
				FilterTool.Passes,
				FilterEquipment.Passes,
				FilterPotion.Passes,
				FilterPlaceable.Passes
		};

		public static bool Passes(Item item)
		{
			foreach (var filter in blacklist)
			{
				if (filter(item))
				{
					return false;
				}
			}
			return true;
		}
	}
}
