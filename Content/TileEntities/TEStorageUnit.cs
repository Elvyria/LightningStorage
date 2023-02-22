using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public class TEStorageUnit : TEStorageComponent
    {
        public bool active = true;
        private Point16 center;

        private List<Item> items = new List<Item>();
        private bool receiving = false;

        private HashSet<ItemData> hasSpaceInStack = new HashSet<ItemData>();
        private HashSet<ItemData> hasItem = new HashSet<ItemData>();

        public int Capacity
        {
            get
            {
                int style = Main.tile[Position.X, Position.Y].TileFrameY / 36;
                if (style == 8)
                {
                    return 4;
                }
                if (style > 1)
                {
                    style--;
                }
                int capacity = style + 1;
                if (capacity > 4)
                {
                    capacity++;
                }
                if (capacity > 6)
                {
                    capacity++;
                }
                if (capacity > 8)
                {
                    capacity += 7;
                }
                return 40 * capacity;
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

        public void Deposit(Item toDeposit)
        {
            Item original = toDeposit.Clone();

            bool finished = false;
            bool hasChange = false;

            foreach (Item item in items)
            {
                if (ItemData.Matches(toDeposit, item) && item.stack < item.maxStack)
                {
                    int total = item.stack + toDeposit.stack;
                    int newStack = total;
                    if (newStack > item.maxStack)
                    {
                        newStack = item.maxStack;
                    }
                    item.stack = newStack;
                    hasChange = true;
                    toDeposit.stack = total - newStack;
                    if (toDeposit.stack <= 0)
                    {
                        toDeposit.SetDefaults(0, true);
                        finished = true;
                        break;
                    }
                }
            }

            if (!finished && !IsFull)
            {
                Item item = toDeposit.Clone();
                item.newAndShiny = false;
                item.favorited = false;
                items.Add(item);
                toDeposit.SetDefaults(0, true);
                hasChange = true;
                finished = true;
            }

            if (hasChange)
            {
                RepairMetadata();
            }
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

            ClearItemsData();
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

        private void ClearItemsData()
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
}
