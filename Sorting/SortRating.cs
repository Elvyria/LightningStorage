using System.Reflection;  
using System.Collections.Generic;

using Terraria.ModLoader;

namespace MagicStorage.Sorting
{
	public static class SortRating {

		public static int[] weaponRatings = new int[0];

		static SortRating()
		{
			var damageClasses =
				(List<DamageClass>) typeof(DamageClassLoader)
				.GetField("DamageClasses", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(null);

			foreach (var c in damageClasses)
			{
				// c.Type
			}

		}

		public const int WeaponMelee       = 0;
		public const int WeaponRanged      = 1;
		public const int WeaponMagic       = 2;
		public const int WeaponSummon      = 3;
		public const int WeaponThrown      = 4;
		public const int Weapon            = 5;

		public const int Ammo              = 8;
		public const int Picksaw           = 9;
		public const int Hamaxe            = 10;
		public const int Pickaxe           = 11;
		public const int Axe               = 12;
		public const int Hammer            = 13;
		public const int Armor             = 14;
		public const int Vanity            = 15;
		public const int Accessory         = 16;
		public const int Hook              = 17;
		public const int Mount             = 18;
		public const int PotionHealth      = 19;
		public const int PotionMana        = 20;
		public const int PotionRestoration = 21;
		public const int PotionBuff        = 22;
		public const int Consumable        = 23;
		public const int Dye               = 24;
		public const int DyeHair           = 25;
		public const int Placeable         = 26;
		public const int Other             = int.MaxValue;
	}
}
