using System.Linq;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common.Players;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities;

public class TEStorageHeart : TEStorageCenter
{
	public List<Point16> remoteAccesses = new List<Point16>();
	private int updateTimer = 0;
	private CompactStage compactStage = CompactStage.Emptying;

	enum CompactStage
	{
		Emptying,
		Defragging,
		Packing,
		Waiting,
	}

	public override bool IsTileValidForEntity(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.TileType == ModContent.TileType<StorageHeart>();
	}

	public override TEStorageHeart GetHeart() => this;

	public IEnumerable<TEStorageUnit> GetStorageUnits()
	{
		return remoteAccesses
			.Select(TileEntity.ByPosition.GetValueOrDefault)
			.OfType<TERemoteAccess>()
			.SelectMany(access => access.storageUnits)
			.Concat(storageUnits)
			.Select(TileEntity.ByPosition.GetValueOrDefault)
			.OfType<TEStorageUnit>();
	}

	public IEnumerable<Item> GetStoredItems()
	{
		return GetStorageUnits().SelectMany(storageUnit => storageUnit.GetItems());
	}

	public override void Update()
	{
		remoteAccesses.RemoveAll(access => !ByPosition.ContainsKey(access) || ByPosition[access] is not TERemoteAccess);

		if (++updateTimer >= 100 && StoragePlayer.LocalPlayer.ViewingStorage() == Point16.NegativeOne)
		{
			updateTimer = 0;
			Compact();
		}
	}

	public void Compact()
	{
        bool v = compactStage switch
		{
			CompactStage.Emptying   => EmptyInactive(),
			CompactStage.Defragging => Defragment(),
			CompactStage.Packing    => PackItems(),
			_                       => false,
		};
	}

	public bool EmptyInactive()
	{
		IEnumerable<TEStorageUnit> units = GetStorageUnits();
		IEnumerable<TEStorageUnit> activeUnits = units.Where(unit => unit.active);
		TEStorageUnit? inactiveUnit = units.FirstOrDefault(unit => !unit.active && !unit.IsEmpty);

		if (inactiveUnit == null)
		{
			compactStage++;
			return false;
		}

		foreach (TEStorageUnit unit in activeUnits)
		{
			if (unit.IsEmpty && unit.Capacity >= inactiveUnit.NumItems)
			{
				TEStorageUnit.SwapItems(inactiveUnit, unit);
				return true;
			}
		}

		bool hasChange = false;

		Item tryMove = inactiveUnit.WithdrawStack();
		int amount = tryMove.stack;

		foreach (TEStorageUnit unit in activeUnits)
		{
			while (unit.HasSpaceFor(tryMove) && amount != 0)
			{
				amount -= unit.Deposit(tryMove, amount);

				if (amount == 0 && !inactiveUnit.IsEmpty)
				{
					tryMove = inactiveUnit.WithdrawStack();
					amount = tryMove.stack;
				}

				hasChange = true;
			}

			if (inactiveUnit.IsEmpty) {
				break;
			}
		}

		if (amount != 0)
		{
			inactiveUnit.Deposit(tryMove, amount);
		}

		if (!hasChange)
		{
			compactStage++;
		}

		return hasChange;
	}

	public bool Defragment()
	{
		IEnumerable<TEStorageUnit> activeUnits = GetStorageUnits().Where(unit => unit.active);
		TEStorageUnit? emptyUnit = activeUnits.FirstOrDefault(unit => unit.IsEmpty);

		if (emptyUnit != null)
		{
			foreach (TEStorageUnit unit in activeUnits)
			{
				if (emptyUnit.Capacity > unit.Capacity && !unit.IsEmpty)
				{
					TEStorageUnit.SwapItems(emptyUnit, unit);
					return true;
				}
			}
		}

		compactStage++;
		return false;
	}

	public bool PackItems()
	{
		IEnumerable<TEStorageUnit> activeUnits = GetStorageUnits().Where(unit => unit.active);
		IEnumerable<TEStorageUnit> spaceUnits = activeUnits.Where(unit => !unit.IsEmpty && !unit.IsFull);
		TEStorageUnit? unitWithSpace = spaceUnits.MaxBy(unit => unit.Capacity);

		if (unitWithSpace == null || spaceUnits.Count() < 2)
		{
			compactStage++;
			return false;
		}

		foreach (TEStorageUnit unit in activeUnits)
		{
			if (unit != unitWithSpace && !unit.IsFull)
			{
				while (!unitWithSpace.IsFull && !unit.IsEmpty)
				{
					Item item = unit.WithdrawStack();
					int deposited = unitWithSpace.Deposit(item, item.stack);
					if (deposited != item.stack)
					{
						unit.Deposit(item, item.stack - deposited);
					}
				}

				return true;
			}
		}

		compactStage++;
		return false;
	}

	public void ResetCompactStage() => compactStage = CompactStage.Emptying;

	public void Deposit(Item deposit)
	{
		if (deposit.IsAir || deposit.stack < 1) return;

		IEnumerable<TEStorageUnit> activeUnits = GetStorageUnits().Where(unit => unit.active);

		int amount = deposit.stack;
		foreach (TEStorageUnit unit in activeUnits)
		{
			amount -= unit.Fill(deposit, amount);
			if (amount == 0) break;
		}

		if (amount != 0)
		{
			foreach (TEStorageUnit unit in activeUnits)
			{
				if (!unit.IsFull)
				{
					amount -= unit.Deposit(deposit, amount);
					if (amount == 0) break;
				}
			}
		}

		if (deposit.stack != amount)
		{
			deposit.stack = amount;

			if (deposit.stack == 0)
			{
				deposit.TurnToAir();
			}

			ResetCompactStage();
		}
	}

	public Item Withdraw(Item lookFor)
	{
		Item result = lookFor.Clone();
		result.stack = 0;

		IEnumerable<TEStorageUnit> units = GetStorageUnits();

		foreach (TEStorageUnit storageUnit in units)
		{
			if (storageUnit.HasItem(lookFor))
			{
				Item withdrawn = storageUnit.Withdraw(lookFor);
				if (!withdrawn.IsAir)
				{
					result.stack += withdrawn.stack;

					if (result.stack == lookFor.stack)
					{
						break;
					}
				}
			}
		}

		if (result.stack == 0)
		{
			return new Item();
		}

		ResetCompactStage();

		return result;
	}

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag);

		List<TagCompound> tags = remoteAccesses.Select(TagExtensions.BuildTag).ToList();
		tag.Set("RemoteAccesses", tags);
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag);

		remoteAccesses =
			tag.GetList<TagCompound>("RemoteAccesses")
			.Select(TagExtensions.GetPoint16)
			.ToList();
	}
}
