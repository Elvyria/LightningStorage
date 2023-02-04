using Terraria;
using Terraria.Audio;
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
				MagicStorageSystem system = ModContent.GetInstance<MagicStorageSystem>();
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
			if (Player.whoAmI != Main.myPlayer)
			{
				return;
			}
			if (timeSinceOpen < 1)
			{
				Player.SetTalkNPC(-1);
				Main.playerInventory = true;
				timeSinceOpen++;
			}
			if (storageAccess.X >= 0 && storageAccess.Y >= 0 && (Player.chest != -1 || !Main.playerInventory || Player.sign > -1 || Player.talkNPC > -1))
			{
				CloseStorage();
				Recipe.FindRecipes();
			}
			else if (storageAccess.X >= 0 && storageAccess.Y >= 0)
			{
				int playerX = (int)(Player.Center.X / 16f);
				int playerY = (int)(Player.Center.Y / 16f);
				if (!remoteAccess && (playerX < storageAccess.X - Player.tileRangeX || playerX > storageAccess.X + Player.tileRangeX + 1 || playerY < storageAccess.Y - Player.tileRangeY || playerY > storageAccess.Y + Player.tileRangeY + 1))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
					Recipe.FindRecipes();
				}
				else if (!(TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].TileType) is StorageAccess))
				{
					SoundEngine.PlaySound(SoundID.MenuClose);
					CloseStorage();
					Recipe.FindRecipes();
				}
			}
		}

		public void OpenStorage(Point16 point, bool remote = false)
		{
			storageAccess = point;
			remoteAccess = remote;
			MagicStorageSystem system = ModContent.GetInstance<MagicStorageSystem>();
			TileEntity tile = TileEntity.ByPosition[storageAccess];
			if (tile is TECraftingAccess)
			{
				system.UI.SetState(system.CraftingUI);
			}
			else if (tile is TEStorageHeart)
			{
				system.UI.SetState(system.StorageUI);
			}
		}

		public void CloseStorage()
		{
			MagicStorageSystem system = ModContent.GetInstance<MagicStorageSystem>();
			system.UI.SetState(null);

			storageAccess = new Point16(-1, -1);
		}

		public Point16 ViewingStorage()
		{
			return storageAccess;
		}

		public static void GetItem(Item item, bool toMouse)
		{
			StoragePlayer storagePlayer = StoragePlayer.LocalPlayer;
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
}
