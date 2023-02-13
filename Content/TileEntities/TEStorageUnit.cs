using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using MagicStorage.Common;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public class TEStorageUnit : TEAbstractStorageUnit
    {
        private IList<Item> items = new List<Item>();
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

        public override bool IsFull
        {
            get
            {
                return items.Count >= Capacity;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return items.Count == 0;
            }
        }

        public int NumItems
        {
            get
            {
                return items.Count;
            }
        }

        public override bool ValidTile(Tile tile)
        {
            return tile.TileType == ModContent.TileType<StorageUnit>() && tile.TileFrameX % 36 == 0 && tile.TileFrameY % 36 == 0;
        }

        public override bool HasSpaceInStackFor(Item check)
        {
            ItemData data = new ItemData(check);
            return hasSpaceInStack.Contains(data);
        }

        public bool HasSpaceFor(Item check)
        {
            return !IsFull || HasSpaceInStackFor(check);
        }

        public override bool HasItem(Item item)
        {
            return hasItem.Contains(new ItemData(item));
        }

        public override IEnumerable<Item> GetItems()
        {
            return items;
        }

        public override void DepositItem(Item toDeposit)
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
                PostChangeContents();
            }
        }

        public override Item TryWithdraw(Item lookFor)
        {
            Item original = lookFor.Clone();
            Item result = lookFor.Clone();
            result.stack = 0;
            for (int k = 0; k < items.Count; k++)
            {
                Item item = items[k];
                if (ItemData.Matches(lookFor, item))
                {
                    int withdraw = Math.Min(lookFor.stack, item.stack);
                    item.stack -= withdraw;
                    if (item.stack <= 0)
                    {
                        items.RemoveAt(k);
                        k--;
                    }
                    result.stack += withdraw;
                    lookFor.stack -= withdraw;
                    if (lookFor.stack <= 0)
                    {
                        PostChangeContents();
                        return result;
                    }
                }
            }
            if (result.stack == 0)
            {
                return new Item();
            }
            PostChangeContents();
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
            if (Inactive)
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
            IList<Item> items = unit1.items;
            unit1.items = unit2.items;
            unit2.items = items;
            HashSet<ItemData> dict = unit1.hasSpaceInStack;
            unit1.hasSpaceInStack = unit2.hasSpaceInStack;
            unit2.hasSpaceInStack = dict;
            dict = unit1.hasItem;
            unit1.hasItem = unit2.hasItem;
            unit2.hasItem = dict;

            unit1.PostChangeContents();
            unit2.PostChangeContents();
        }

        internal Item WithdrawStack()
        {
            Item item = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            PostChangeContents();
            return item;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);

            List<TagCompound> tagItems = new List<TagCompound>(items.Count);
            foreach (Item item in items)
            {
                tagItems.Add(ItemIO.Save(item));
            }

            tag.Set("Items", tagItems);
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            ClearItemsData();
            foreach (TagCompound tagItem in tag.GetList<TagCompound>("Items"))
            {
                Item item = ItemIO.Load(tagItem);
                items.Add(item);
                ItemData data = new ItemData(item);
                if (item.stack < item.maxStack)
                {
                    hasSpaceInStack.Add(data);
                }
                hasItem.Add(data);
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

        private void PostChangeContents()
        {
            RepairMetadata();
            UpdateTileFrame();
        }
    }
}
