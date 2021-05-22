using System;
using System.Collections.Generic;

using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

using MagicStorage.Components;

namespace MagicStorage
{
	public class StoragePlayer : ModPlayer
	{
		public static StoragePlayer LocalPlayer => Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>();

		public int timeSinceOpen = 1;
		public bool remoteAccess = false;

		private Point16 storageAccess = new Point16(-1, -1);

		public void RefreshItems()
		{
			if (storageAccess.X != -1)
			{
				TileEntity tile = TileEntity.ByPosition[storageAccess];
				if (tile is TECraftingAccess)
				{
					MagicStorage.Instance.CraftingUI.Refresh();
				}
				else if (tile is TEStorageHeart)
				{
					MagicStorage.Instance.StorageUI.RefreshItems();
				}
			}
		}

		public override void UpdateDead()
		{
			if (player.whoAmI == Main.myPlayer)
			{
				CloseStorage();
			}
		}

		public override void ResetEffects()
		{
			if (player.whoAmI != Main.myPlayer)
			{
				return;
			}
			if (timeSinceOpen < 1)
			{
				player.talkNPC = -1;
				Main.playerInventory = true;
				timeSinceOpen++;
			}
			if (storageAccess.X >= 0 && storageAccess.Y >= 0 && (player.chest != -1 || !Main.playerInventory || player.sign > -1 || player.talkNPC > -1))
			{
				CloseStorage();
				Recipe.FindRecipes();
			}
			else if (storageAccess.X >= 0 && storageAccess.Y >= 0)
			{
				int playerX = (int)(player.Center.X / 16f);
				int playerY = (int)(player.Center.Y / 16f);
				if (!remoteAccess && (playerX < storageAccess.X - Player.tileRangeX || playerX > storageAccess.X + Player.tileRangeX + 1 || playerY < storageAccess.Y - Player.tileRangeY || playerY > storageAccess.Y + Player.tileRangeY + 1))
				{
					Main.PlaySound(11, -1, -1, 1);
					CloseStorage();
					Recipe.FindRecipes();
				}
				else if (!(TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].type) is StorageAccess))
				{
					Main.PlaySound(11, -1, -1, 1);
					CloseStorage();
					Recipe.FindRecipes();
				}
			}
		}

		public void OpenStorage(Point16 point, bool remote = false)
		{
			storageAccess = point;
			remoteAccess = remote;
			TileEntity tile = TileEntity.ByPosition[storageAccess];
			if (tile is TECraftingAccess)
			{
				MagicStorage.Instance.UI.SetState(MagicStorage.Instance.CraftingUI);
			}
			else if (tile is TEStorageHeart)
			{
				MagicStorage.Instance.UI.SetState(MagicStorage.Instance.StorageUI);
			}
		}

		public void CloseStorage()
		{
			MagicStorage.Instance.UI?.SetState(null);

			storageAccess = new Point16(-1, -1);
		}

		public Point16 ViewingStorage()
		{
			return storageAccess;
		}

		public static void GetItem(Item item, bool toMouse)
		{
			Player player = Main.player[Main.myPlayer];
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
				item = player.GetItem(Main.myPlayer, item, false, true);
				if (!item.IsAir)
				{
					player.QuickSpawnClonedItem(item, item.stack);
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
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					GetCraftingAccess().TryDepositStation(item);
				}
				else
				{
					NetHelper.SendDepositStation(GetCraftingAccess().ID, item);
					item.SetDefaults(0, true);
				}
			}
			else
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					GetStorageHeart().DepositItem(item);
				}
				else
				{
					NetHelper.SendDeposit(GetStorageHeart().ID, item);
					item.SetDefaults(0, true);
				}
			}
			if (item.type != oldType || item.stack != oldStack)
			{
				Main.PlaySound(7, -1, -1, 1, 1f, 0f);
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
			int tileType = tile.type;
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
			return tile != null && tile.type == mod.TileType("CraftingAccess");
		}
	}
}
