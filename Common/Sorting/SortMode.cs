namespace MagicStorage.Common.Sorting
{
    public static class SortMode
    {
        public static readonly IComparer<Item> Default = new SortDefault();
        public static readonly IComparer<Item> Id = new SortID();
        public static readonly IComparer<Item> Name = new SortName();
        public static readonly IComparer<Item> Quantity = new SortQuantity();
        public static readonly IComparer<Item> Power = null;
        public static readonly IComparer<Item> Damage = null;

        private static readonly IComparer<Item>[] indices = new[] {
            Default,
            Id,
            Name,
            Quantity
        };

        public static int index(this IComparer<Item> comparer)
        {
            return Array.IndexOf(indices, comparer);
        }

        public static IComparer<Item> from(int i)
        {
            if (i < indices.Length)
                return indices[i];

            return indices[0];
        }

        public class SortDefault : IComparer<Item>
        {
            int IComparer<Item>.Compare(Item a, Item b)
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
                        return SortRating.WeaponMelee;
                    else if (item.CountsAsClass(DamageClass.Ranged))
                        return SortRating.WeaponRanged;
                    else if (item.CountsAsClass(DamageClass.Magic))
                        return SortRating.WeaponMagic;
                    else if (item.CountsAsClass(DamageClass.Summon))
                        return SortRating.WeaponSummon;
                    else if (item.CountsAsClass(DamageClass.Throwing))
                        return SortRating.WeaponThrown;
                    else
                        return SortRating.Weapon;
                }

                if (Tool(item))
                {
                    if (item.axe > 0 && item.pick > 0)
                        return SortRating.Picksaw;
                    else if (item.axe > 0 && item.hammer > 0)
                        return SortRating.Hamaxe;
                    else if (item.pick > 0)
                        return SortRating.Pickaxe;
                    else if (item.axe > 0)
                        return SortRating.Axe;
                    else if (item.hammer > 0)
                        return SortRating.Hammer;
                }

                if (item.consumable)
                {
                    if (item.healLife > 0 && item.healMana < 1)
                        return SortRating.PotionHealth;
                    else if (item.healMana > 0 && item.healLife < 1)
                        return SortRating.PotionMana;
                    else if ((item.healLife & item.healMana) > 0)
                        return SortRating.PotionRestoration;

                    return SortRating.Consumable;
                }

                if (item.ammo > 0 && item.damage > 0)
                    return SortRating.Ammo;
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
                return (item.axe | item.pick | item.hammer) > 0;
            }

            static bool Hook(Item item)
            {
                return Main.projHook[item.shoot];
            }

            static bool Armor(Item item)
            {
                return !item.vanity && (item.headSlot | item.bodySlot | item.legSlot) > 0;
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
                return -a.stack.CompareTo(b.stack);
            }
        }

        public class SortType : IComparer<Item>
        {
            int IComparer<Item>.Compare(Item a, Item b)
            {
                return a.type.CompareTo(b.type);
            }
        }
    }
}
