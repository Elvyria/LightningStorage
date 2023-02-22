using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common.Players;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities;

public class TEStorageHeart : TEStorageCenter
{
	public List<Point16> remoteAccesses = new List<Point16>();
	private int updateTimer = 60;
	private int compactStage = 0;

	public override bool IsTileValidForEntity(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.TileType == ModContent.TileType<StorageHeart>();
	}

	public override TEStorageHeart GetHeart()
	{
		return this;
	}

	public IEnumerable<TEStorageUnit> GetStorageUnits()
	{
		return storageUnits
			.Concat(remoteAccesses.Where(remoteAccess => ByPosition.ContainsKey(remoteAccess) && ByPosition[remoteAccess] is TERemoteAccess)
			.SelectMany(remoteAccess => ((TERemoteAccess)ByPosition[remoteAccess]).storageUnits))
			.Where(storageUnit => ByPosition.ContainsKey(storageUnit) && ByPosition[storageUnit] is TEStorageUnit)
			.Select(storageUnit => (TEStorageUnit)ByPosition[storageUnit]);
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

		if (updateTimer < 60)
		{
			updateTimer++;
		}

		if (updateTimer == 60 && StoragePlayer.LocalPlayer.ViewingStorage() == Point16.NegativeOne)
		{
			updateTimer = 0;
			Compact();
		}
	}

	public void Compact()
	{
		switch (compactStage)
		{
			case 0: EmptyInactive(); break;
			case 1: Defragment();    break;
			case 2: PackItems();     break;
		}
	}

	public bool EmptyInactive()
	{
		TEStorageUnit inactiveUnit = null;

		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (!storageUnit.active && !storageUnit.IsEmpty)
			{
				inactiveUnit = storageUnit;
			}
		}

		if (inactiveUnit == null)
		{
			compactStage++;
			return false;
		}

		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (storageUnit.IsEmpty && inactiveUnit.NumItems <= storageUnit.Capacity)
			{
				TEStorageUnit.SwapItems(inactiveUnit, storageUnit);
				return true;
			}
		}
		bool hasChange = false;
		Item tryMove = inactiveUnit.WithdrawStack();
		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (storageUnit.active)
			{
				while (storageUnit.HasSpaceFor(tryMove) && !tryMove.IsAir)
				{
					storageUnit.Deposit(tryMove);
					if (tryMove.IsAir && !inactiveUnit.IsEmpty)
					{
						tryMove = inactiveUnit.WithdrawStack();
					}
					hasChange = true;
				}
			}
		}
		if (!tryMove.IsAir)
		{
			inactiveUnit.Deposit(tryMove);
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
		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (emptyUnit == null && storageUnit.IsEmpty && storageUnit.active)
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
		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (unitWithSpace == null && !storageUnit.IsFull && storageUnit.active)
			{
				unitWithSpace = storageUnit;
			}
			else if (unitWithSpace != null && !storageUnit.IsEmpty)
			{
				while (!unitWithSpace.IsFull && !storageUnit.IsEmpty)
				{
					Item item = storageUnit.WithdrawStack();
					unitWithSpace.Deposit(item);
					if (!item.IsAir)
					{
						storageUnit.Deposit(item);
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
		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (storageUnit.active && storageUnit.HasSpaceInStackFor(toDeposit))
			{
				storageUnit.Deposit(toDeposit);
				if (toDeposit.IsAir)
				{
					return;
				}
			}
		}
		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (storageUnit.active && !storageUnit.IsFull)
			{
				storageUnit.Deposit(toDeposit);
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

		foreach (TEStorageUnit storageUnit in GetStorageUnits())
		{
			if (storageUnit.HasItem(lookFor))
			{
				Item withdrawn = storageUnit.Withdraw(lookFor);
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

					if (result.stack == lookFor.stack)
					{
						break;
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

		List<TagCompound> tagRemotes = new List<TagCompound>(remoteAccesses.Count);
		foreach (Point16 remoteAccess in remoteAccesses)
		{
			tagRemotes.Add(new TagCompound()
					{
					{ "X", remoteAccess.X },
					{ "Y", remoteAccess.Y }
					});
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
