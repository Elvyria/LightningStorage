using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities
{
    public class TEStorageHeart : TEStorageCenter
    {
        public List<Point16> remoteAccesses = new List<Point16>();
        private int updateTimer = 60;
        private int compactStage = 0;

        public override bool ValidTile(Tile tile)
        {
            return tile.TileType == ModContent.TileType<StorageHeart>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override TEStorageHeart GetHeart()
        {
            return this;
        }

        public IEnumerable<TEAbstractStorageUnit> GetStorageUnits()
        {
            return storageUnits
                .Concat(remoteAccesses.Where(remoteAccess => ByPosition.ContainsKey(remoteAccess) && ByPosition[remoteAccess] is TERemoteAccess)
                .SelectMany(remoteAccess => ((TERemoteAccess)ByPosition[remoteAccess]).storageUnits))
                .Where(storageUnit => ByPosition.ContainsKey(storageUnit) && ByPosition[storageUnit] is TEAbstractStorageUnit)
                .Select(storageUnit => (TEAbstractStorageUnit)ByPosition[storageUnit]);
        }

        public IEnumerable<Item> GetStoredItems()
        {
            return GetStorageUnits().SelectMany(storageUnit => storageUnit.GetItems());
        }

        public override void Update()
        {
            for (int k = 0; k < remoteAccesses.Count; k++)
            {
                if (!ByPosition.ContainsKey(remoteAccesses[k]) || !(ByPosition[remoteAccesses[k]] is TERemoteAccess))
                {
                    remoteAccesses.RemoveAt(k);
                    k--;
                }
            }
            updateTimer++;
            if (updateTimer >= 60)
            {
                updateTimer = 0;
                CompactOne();
            }
        }

        public void CompactOne()
        {
            if (compactStage == 0)
            {
                EmptyInactive();
            }
            else if (compactStage == 1)
            {
                Defragment();
            }
            else if (compactStage == 2)
            {
                PackItems();
            }
        }

        public bool EmptyInactive()
        {
            TEStorageUnit inactiveUnit = null;
            foreach (TEAbstractStorageUnit abstractStorageUnit in GetStorageUnits())
            {
                if (!(abstractStorageUnit is TEStorageUnit))
                {
                    continue;
                }
                TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
                if (storageUnit.Inactive && !storageUnit.IsEmpty)
                {
                    inactiveUnit = storageUnit;
                }
            }
            if (inactiveUnit == null)
            {
                compactStage++;
                return false;
            }
            foreach (TEAbstractStorageUnit abstractStorageUnit in GetStorageUnits())
            {
                if (!(abstractStorageUnit is TEStorageUnit) || abstractStorageUnit.Inactive)
                {
                    continue;
                }
                TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
                if (storageUnit.IsEmpty && inactiveUnit.NumItems <= storageUnit.Capacity)
                {
                    TEStorageUnit.SwapItems(inactiveUnit, storageUnit);
                    return true;
                }
            }
            bool hasChange = false;
            Item tryMove = inactiveUnit.WithdrawStack();
            foreach (TEAbstractStorageUnit abstractStorageUnit in GetStorageUnits())
            {
                if (!(abstractStorageUnit is TEStorageUnit) || abstractStorageUnit.Inactive)
                {
                    continue;
                }
                TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
                while (storageUnit.HasSpaceFor(tryMove) && !tryMove.IsAir)
                {
                    storageUnit.DepositItem(tryMove);
                    if (tryMove.IsAir && !inactiveUnit.IsEmpty)
                    {
                        tryMove = inactiveUnit.WithdrawStack();
                    }
                    hasChange = true;
                }
            }
            if (!tryMove.IsAir)
            {
                inactiveUnit.DepositItem(tryMove);
            }
            if (!hasChange)
            {
                compactStage++;
            }
            return hasChange;
        }

        public bool Defragment()
        {
            TEStorageUnit emptyUnit = null;
            foreach (TEAbstractStorageUnit abstractStorageUnit in GetStorageUnits())
            {
                if (!(abstractStorageUnit is TEStorageUnit))
                {
                    continue;
                }
                TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
                if (emptyUnit == null && storageUnit.IsEmpty && !storageUnit.Inactive)
                {
                    emptyUnit = storageUnit;
                }
                else if (emptyUnit != null && !storageUnit.IsEmpty && storageUnit.NumItems <= emptyUnit.Capacity)
                {
                    TEStorageUnit.SwapItems(emptyUnit, storageUnit);
                    return true;
                }
            }
            compactStage++;
            return false;
        }

        public bool PackItems()
        {
            TEStorageUnit unitWithSpace = null;
            foreach (TEAbstractStorageUnit abstractStorageUnit in GetStorageUnits())
            {
                if (!(abstractStorageUnit is TEStorageUnit))
                {
                    continue;
                }
                TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
                if (unitWithSpace == null && !storageUnit.IsFull && !storageUnit.Inactive)
                {
                    unitWithSpace = storageUnit;
                }
                else if (unitWithSpace != null && !storageUnit.IsEmpty)
                {
                    while (!unitWithSpace.IsFull && !storageUnit.IsEmpty)
                    {
                        Item item = storageUnit.WithdrawStack();
                        unitWithSpace.DepositItem(item);
                        if (!item.IsAir)
                        {
                            storageUnit.DepositItem(item);
                        }
                    }
                    return true;
                }
            }
            compactStage++;
            return false;
        }

        public void ResetCompactStage(int stage = 0)
        {
            if (stage < compactStage)
            {
                compactStage = stage;
            }
        }

        public void DepositItem(Item toDeposit)
        {
            int oldStack = toDeposit.stack;
            foreach (TEAbstractStorageUnit storageUnit in GetStorageUnits())
            {
                if (!storageUnit.Inactive && storageUnit.HasSpaceInStackFor(toDeposit))
                {
                    storageUnit.DepositItem(toDeposit);
                    if (toDeposit.IsAir)
                    {
                        return;
                    }
                }
            }
            foreach (TEAbstractStorageUnit storageUnit in GetStorageUnits())
            {
                if (!storageUnit.Inactive && !storageUnit.IsFull)
                {
                    storageUnit.DepositItem(toDeposit);
                    if (toDeposit.IsAir)
                    {
                        return;
                    }
                }
            }

            if (oldStack != toDeposit.stack)
            {
                ResetCompactStage();
            }
        }

        public Item TryWithdraw(Item lookFor)
        {
            Item result = new Item();
            foreach (TEAbstractStorageUnit storageUnit in GetStorageUnits())
            {
                if (storageUnit.HasItem(lookFor))
                {
                    Item withdrawn = storageUnit.TryWithdraw(lookFor);
                    if (!withdrawn.IsAir)
                    {
                        if (result.IsAir)
                        {
                            result = withdrawn;
                        }
                        else
                        {
                            result.stack += withdrawn.stack;
                        }
                        if (lookFor.stack <= 0)
                        {
                            ResetCompactStage();
                            return result;
                        }
                    }
                }
            }
            if (result.stack > 0)
            {
                ResetCompactStage();
            }
            return result;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);

            List<TagCompound> tagRemotes = new List<TagCompound>();
            foreach (Point16 remoteAccess in remoteAccesses)
            {
                TagCompound tagRemote = new TagCompound();
                tagRemote.Set("X", remoteAccess.X);
                tagRemote.Set("Y", remoteAccess.Y);
                tagRemotes.Add(tagRemote);
            }

            tag.Set("RemoteAccesses", tagRemotes);
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);

            foreach (TagCompound tagRemote in tag.GetList<TagCompound>("RemoteAccesses"))
            {
                remoteAccesses.Add(new Point16(tagRemote.GetShort("X"), tagRemote.GetShort("Y")));
            }
        }
    }
}
