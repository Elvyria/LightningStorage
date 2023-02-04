using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage.Items
{
	public class PortableAccess : Locator
	{

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.maxStack = 1;
			Item.rare = 11;
			Item.useStyle = 1;
			Item.useAnimation = 28;
			Item.useTime = 28;
			Item.value = Item.sellPrice(0, 10, 0, 0);
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				if (location.X >= 0 && location.Y >= 0)
				{
					Tile tile = Main.tile[location.X, location.Y];
					if (!tile.HasTile || tile.TileType != ModContent.TileType<Components.StorageHeart>() || tile.TileFrameX != 0 || tile.TileFrameY != 0)
					{
						Main.NewText("Storage Heart is missing!");
					}
					else
					{
						OpenStorage(player);
					}
				}
				else
				{
					Main.NewText("Locator is not set to any Storage Heart");
				}
			}
			return true;
		}

		private void OpenStorage(Player player)
		{
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			if (player.sign > -1)
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = string.Empty;
			}
			if (Main.editChest)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = string.Empty;
			}
			if (player.editedChestName)
			{
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
				player.editedChestName = false;
			}
			if (player.talkNPC > -1)
			{
				player.SetTalkNPC(-1);
				Main.npcChatCornerItem = 0;
				Main.npcChatText = string.Empty;
			}
			bool hadChestOpen = player.chest != -1;
			player.chest = -1;
			Main.stackSplit = 600;
			Point16 toOpen = location;
			Point16 prevOpen = modPlayer.ViewingStorage();
			if (prevOpen == toOpen)
			{
				modPlayer.CloseStorage();
				SoundEngine.PlaySound(SoundID.MenuClose);
				Recipe.FindRecipes();
			}
			else
			{
				bool hadOtherOpen = prevOpen.X >= 0 && prevOpen.Y >= 0;
				modPlayer.OpenStorage(toOpen, true);
				modPlayer.timeSinceOpen = 0;
				Main.playerInventory = true;
				Main.recBigList = false;
				SoundEngine.PlaySound(hadChestOpen || hadOtherOpen ? SoundID.MenuTick : SoundID.MenuOpen);
				Recipe.FindRecipes();
			}
		}

		public override void ModifyTooltips(List<TooltipLine> lines)
		{
			bool isSet = location.X >= 0 && location.Y >= 0;
			for (int k = 0; k < lines.Count; k++)
			{
				if (isSet && lines[k].Mod == "Terraria" && lines[k].Name == "Tooltip1")
				{
					lines[k].Text = Language.GetTextValue("Mods.MagicStorage.Common.SetTo", location.X, location.Y);
				}
				else if (!isSet && lines[k].Mod == "Terraria" && lines[k].Name == "Tooltip2")
				{
					lines.RemoveAt(k);
					k--;
				}
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<LocatorDisk>()
				.AddIngredient(ItemID.Diamond, 3)
				.AddIngredient(ItemID.Ruby, 7)
				.AddTile(TileID.LunarCraftingStation)
				.Register();

			if (ModLoader.TryGetMod("CalamityMod", out var calamityMod))
			{
				CreateRecipe()
					.AddIngredient<LocatorDisk>()
					.AddIngredient(calamityMod, "CosmiliteBar", 20)
					.AddTile(TileID.LunarCraftingStation)
					.Register();
			}
		}
	}
}
