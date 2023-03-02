using Terraria.DataStructures;

using MagicStorage.Common.UI;
using MagicStorage.Common.Systems;
using MagicStorage.Content.TileEntities;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Common.Players;

public class StoragePlayer : ModPlayer
{
	public static StoragePlayer LocalPlayer => Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>();

	public int timeSinceOpen = 1;
	public bool remoteAccess = false;

	private Point16 accessPosition = Point16.NegativeOne;

	public void RefreshItems()
	{
		if (accessPosition != Point16.NegativeOne)
		{
			UISystem system = ModContent.GetInstance<UISystem>();
			TileEntity tile = TileEntity.ByPosition[accessPosition];
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

		if (accessPosition != Point16.NegativeOne)
		{
			if (Player.chest != -1 || !Main.playerInventory || Player.sign > -1 || Player.talkNPC > -1)
			{
				CloseStorage();
			}
			else
			{
				int playerX = (int)(Player.Center.X / 16f);
				int playerY = (int)(Player.Center.Y / 16f);
				if (!remoteAccess && (playerX < accessPosition.X - Player.tileRangeX || playerX > accessPosition.X + Player.tileRangeX + 1 || playerY < accessPosition.Y - Player.tileRangeY || playerY > accessPosition.Y + Player.tileRangeY + 1))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
				}
				else if (!(TileLoader.GetTile(Main.tile[accessPosition.X, accessPosition.Y].TileType) is StorageAccess))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
				}
			}
		}
	}

	public void OpenStorage(Point16 point, bool remote = false)
	{
		if (point == accessPosition)
		{
			remoteAccess = remote;
			return;
		}

		accessPosition = point;
		remoteAccess = remote;
		timeSinceOpen = 0;

		UISystem system = ModContent.GetInstance<UISystem>();
		ModTile tile = TileLoader.GetTile(Main.tile[accessPosition.X, accessPosition.Y].TileType);

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
			system.CraftingUI.Open();
		}
		else if (tile is StorageAccess)
		{
			system.StorageUI.Open(true);
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
	}

	public void CloseStorage()
	{
		accessPosition = Point16.NegativeOne;

		UISystem system = ModContent.GetInstance<UISystem>();

		if (system.UI.CurrentState is ISwitchable ui)
		{
			ui.Close(true);
		}
		else
		{
			system.UI.SetState(null);
		}

		SoundEngine.PlaySound(SoundID.MenuClose);
	}

	public Point16 ViewingStorage() => accessPosition;

	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		Item item = inventory[slot];
		if (item.favorited || item.IsAir)
		{
			return false;
		}

		if (context != ItemSlot.Context.InventoryItem && context != ItemSlot.Context.InventoryCoin && context != ItemSlot.Context.InventoryAmmo)
		{
			return false;
		}
		if (accessPosition.X < 0 || accessPosition.Y < 0)
		{
			return false;
		}
		int oldType = item.type;
		int oldStack = item.stack;
		if (StorageCrafting())
		{
			GetCraftingAccess()?.DepositStation(item);
		}
		else
		{
			GetStorageHeart()?.Deposit(item);
		}

		if (item.type != oldType || item.stack != oldStack)
		{
			SoundEngine.PlaySound(SoundID.Grab);
			RefreshItems();
		}

		return true;
	}

	public TEStorageHeart? GetStorageHeart()
	{
		if (accessPosition == Point16.NegativeOne)
		{
			return null;
		}

		Tile tile = Main.tile[accessPosition.X, accessPosition.Y];
		if (tile == null)
		{
			return null;
		}

		ModTile modTile = TileLoader.GetTile(tile.TileType);
		if (modTile is StorageAccess storageAccess)
		{
			return storageAccess.GetHeart(accessPosition.X, accessPosition.Y);
		}

		return null;
	}

	public TECraftingAccess? GetCraftingAccess()
	{
		if (accessPosition != Point16.NegativeOne && TileEntity.ByPosition.ContainsKey(accessPosition) && TileEntity.ByPosition[accessPosition] is TECraftingAccess craftingAccess)
		{
			return craftingAccess;
		}

		return null;
	}

	public bool StorageCrafting()
	{
		if (accessPosition == Point16.NegativeOne)
		{
			return false;
		}

		Tile tile = Main.tile[accessPosition.X, accessPosition.Y];
		return tile != null && tile.TileType == ModContent.TileType<CraftingAccess>();
	}
}
