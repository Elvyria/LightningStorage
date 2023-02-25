using Terraria.DataStructures;
using MagicStorage.Common.Systems;
using MagicStorage.Content.TileEntities;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Common.Players;

public class StoragePlayer : ModPlayer
{
	public static StoragePlayer LocalPlayer => Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>();

	public int timeSinceOpen = 1;
	public bool remoteAccess = false;

	private Point16 storageAccess = Point16.NegativeOne;

	public void RefreshItems()
	{
		if (storageAccess.X != -1)
		{
			UISystem system = ModContent.GetInstance<UISystem>();
			TileEntity tile = TileEntity.ByPosition[storageAccess];
			if (tile is TECraftingAccess)
			{
				system.CraftingUI.Refresh();
			}
			else if (tile is TEStorageHeart)
			{
				system.StorageUI.RefreshItems();
			}
		}
	}

	public override void UpdateDead()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			CloseStorage();
		}
	}

	public override void ResetEffects()
	{
		if (timeSinceOpen < 1)
		{
			Player.SetTalkNPC(-1);
			Main.playerInventory = true;
			timeSinceOpen++;
		}
		if (storageAccess.X >= 0 && storageAccess.Y >= 0)
		{
			if (Player.chest != -1 || !Main.playerInventory || Player.sign > -1 || Player.talkNPC > -1)
			{
				CloseStorage();
			}
			else
			{
				int playerX = (int)(Player.Center.X / 16f);
				int playerY = (int)(Player.Center.Y / 16f);
				if (!remoteAccess && (playerX < storageAccess.X - Player.tileRangeX || playerX > storageAccess.X + Player.tileRangeX + 1 || playerY < storageAccess.Y - Player.tileRangeY || playerY > storageAccess.Y + Player.tileRangeY + 1))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
				}
				else if (!(TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].TileType) is StorageAccess))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
				}
			}
		}
	}

	public void OpenStorage(Point16 point, bool remote = false)
	{
		if (point == storageAccess)
		{
			remoteAccess = remote;
			return;
		}

		storageAccess = point;
		remoteAccess = remote;
		timeSinceOpen = 0;

		UISystem system = ModContent.GetInstance<UISystem>();
		ModTile tile = TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].TileType);

		Main.playerInventory = true;
		Main.editChest = false;
		Main.recBigList = false;

		Player.chest = -1;

		if (Player.talkNPC > -1)
		{
			Player.SetTalkNPC(-1);
			Main.npcChatCornerItem = 0;
			Main.npcChatText = string.Empty;
		}

		if (Main.editSign)
		{
			Player.sign = -1;
			Main.editSign = false;
			Main.npcChatText = string.Empty;
		}

		if (Main.editChest)
		{
			Main.editChest = false;
			Main.npcChatText = string.Empty;
		}

		if (tile is CraftingAccess)
		{
			system.UI.SetState(system.CraftingUI);
		}
		else if (tile is StorageAccess)
		{
			system.UI.SetState(system.StorageUI);
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
	}

	public void CloseStorage()
	{
		storageAccess = Point16.NegativeOne;

		UISystem system = ModContent.GetInstance<UISystem>();
		system.UI.SetState(null);

		storageAccess = Point16.NegativeOne;

		Recipe.FindRecipes();
	}

	public Point16 ViewingStorage()
	{
		return storageAccess;
	}

	public static void GetItem(Item item, bool toMouse)
	{
		StoragePlayer storagePlayer = LocalPlayer;
		Player player = storagePlayer.Player;

		if (toMouse && Main.playerInventory && Main.mouseItem.IsAir)
		{
			Main.mouseItem = item;
			item = new Item();
		}
		else if (toMouse && Main.playerInventory && Main.mouseItem.type == item.type)
		{
			int total = Main.mouseItem.stack + item.stack;
			if (total > Main.mouseItem.maxStack)
			{
				total = Main.mouseItem.maxStack;
			}
			int difference = total - Main.mouseItem.stack;
			Main.mouseItem.stack = total;
			item.stack -= difference;
		}
		if (!item.IsAir)
		{
			item = player.GetItem(Main.myPlayer, item, GetItemSettings.InventoryEntityToPlayerInventorySettings);
			if (!item.IsAir)
			{
				Point16 access = storagePlayer.ViewingStorage();
				player.QuickSpawnClonedItem(new EntitySource_TileEntity(TileEntity.ByPosition[access]), item, item.stack);
			}
		}
	}

	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		if (context != ItemSlot.Context.InventoryItem && context != ItemSlot.Context.InventoryCoin && context != ItemSlot.Context.InventoryAmmo)
		{
			return false;
		}
		if (storageAccess.X < 0 || storageAccess.Y < 0)
		{
			return false;
		}
		Item item = inventory[slot];
		if (item.favorited || item.IsAir)
		{
			return false;
		}
		int oldType = item.type;
		int oldStack = item.stack;
		if (StorageCrafting())
		{
			GetCraftingAccess().TryDepositStation(item);
		}
		else
		{
			GetStorageHeart().DepositItem(item);
		}
		if (item.type != oldType || item.stack != oldStack)
		{
			SoundEngine.PlaySound(SoundID.Grab);
			RefreshItems();
		}
		return true;
	}

	public TEStorageHeart GetStorageHeart()
	{
		if (storageAccess.X < 0 || storageAccess.Y < 0)
		{
			return null;
		}
		Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
		if (tile == null)
		{
			return null;
		}
		int tileType = tile.TileType;
		ModTile modTile = TileLoader.GetTile(tileType);
		if (modTile == null || !(modTile is StorageAccess))
		{
			return null;
		}
		return ((StorageAccess)modTile).GetHeart(storageAccess.X, storageAccess.Y);
	}

	public TECraftingAccess GetCraftingAccess()
	{
		if (storageAccess.X < 0 || storageAccess.Y < 0 || !TileEntity.ByPosition.ContainsKey(storageAccess))
		{
			return null;
		}
		return TileEntity.ByPosition[storageAccess] as TECraftingAccess;
	}

	public bool StorageCrafting()
	{
		if (storageAccess.X < 0 || storageAccess.Y < 0)
		{
			return false;
		}
		Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
		return tile != null && tile.TileType == ModContent.TileType<CraftingAccess>();
	}
}
