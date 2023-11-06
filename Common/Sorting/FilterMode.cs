namespace MagicStorage.Common.Sorting;

#nullable disable
public class FilterMode : ILoadable
{
	public static IFilter<Item> All;
	public static IFilter<Item> Weapons;
	public static IFilter<Item> Tools;
	public static IFilter<Item> Equipment;
	public static IFilter<Item> Potions;
	public static IFilter<Item> Placeables;
	public static IFilter<Item> Misc;

	private static IFilter<Item>[] indices;

    public void Load(Mod mod)
    {
		All        = new FilterAll();
		Weapons    = new FilterWeapon();
		Tools      = new FilterTool();
		Equipment  = new FilterEquipment();
		Potions    = new FilterPotion();
		Placeables = new FilterPlaceable();
		Misc       = new FilterMisc();

		indices = new[] { All, Weapons, Tools, Equipment, Potions, Placeables, Misc };
    }

    public void Unload()
    {
		All = null;
		Weapons = null;
		Tools = null;
		Equipment = null;
		Potions = null;
		Placeables = null;
		Misc = null;

		indices = null;
    }

	public static int index(IFilter<Item> comparer)
	{
		return Array.IndexOf(indices, comparer);
	}

	public static IFilter<Item> from(int i)
	{
		if (i < indices.Length)
			return indices[i];

		return indices[0];
	}

    public class FilterAll : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return true;
		}
	}

	public class FilterMelee : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Melee)
				&& (item.pick & item.axe & item.hammer) == 0;
		}
	}

	public class FilterRanged : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Ranged);
		}
	}

	public class FilterMagic : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Magic);
		}
	}

	public class FilterSummon : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Summon);
		}
	}

	public class FilterThrown : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.CountsAsClass(DamageClass.Throwing);
		}
	}

	public class FilterOtherWeapon : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.damage > 0
				&& !item.CountsAsClass(DamageClass.Melee)
				&& !item.CountsAsClass(DamageClass.Ranged)
				&& !item.CountsAsClass(DamageClass.Magic)
				&& !item.CountsAsClass(DamageClass.Summon)
				&& !item.CountsAsClass(DamageClass.Throwing);
		}
	}

	public class FilterWeapon : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.damage > 0 && !item.accessory && !Tools.Passes(item);
		}
	}

	public class FilterPickaxe : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.pick > 0;
		}
	}

	public class FilterAxe : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.axe > 0;
		}
	}

	public class FilterHammer : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.hammer > 0;
		}
	}

	public class FilterTool : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return (item.pick | item.axe | item.hammer) > 0;
		}
	}

	public class FilterEquipment : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.headSlot >= 0
				|| item.bodySlot >= 0
				|| item.legSlot >= 0
				|| item.accessory
				|| item.shoot > 0 && item.useAmmo == AmmoID.None && Main.projHook[item.shoot]
				|| item.mountType >= 0
				|| item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType]);
		}
	}

	public class FilterPotion : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.consumable && (item.healLife | item.healMana | item.buffType) > 0;
		}
	}

	public class FilterPlaceable : IFilter<Item>
	{
		bool IFilter<Item>.Passes(Item item)
		{
			return item.createTile >= 0 || item.createWall > 0;
		}
	}

	public class FilterMisc : IFilter<Item>
	{
		private readonly IFilter<Item>[] blacklist = new[] {
			Weapons,
			Tools,
			Equipment,
			Potions,
			Placeables,
		};

		bool IFilter<Item>.Passes(Item item)
		{
			foreach (var filter in blacklist)
			{
				if (filter.Passes(item))
				{
					return false;
				}
			}
			return true;
		}
	}
}
