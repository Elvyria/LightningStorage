using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common.Players;
using MagicStorage.Common.UI.States;
using MagicStorage.Common.Systems;

namespace MagicStorage.Content.Items;

public class PortableAccess : ModItem
{
	public Point16 storage = Point16.NegativeOne;
	public Point16 crafting = Point16.NegativeOne;

	public override void SetDefaults()
	{
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = 1;
		Item.rare = ItemRarityID.Purple;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.useAnimation = 28;
		Item.useTime = 28;
		Item.value = Item.sellPrice(gold: 10);
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			int i = (int)(((float)Main.mouseX + Main.screenPosition.X) / 16f);
			int j = (int)(((float)Main.mouseY + Main.screenPosition.Y) / 16f);

			Tile tile = Main.tile[i, j];

			if (tile.TileFrameX % 36 == 18)
			{
				i--;
			}
			if (tile.TileFrameY % 36 == 18)
			{
				j--;
			}

			if (tile.HasTile && tile.TileType == ModContent.TileType<Tiles.StorageHeart>())
			{
				storage = new Point16(i, j);
				Main.NewText("Portable access successfully set to: X=" + i + ", Y=" + j);
			}

			if (tile.HasTile && tile.TileType == ModContent.TileType<Tiles.CraftingAccess>())
			{
				crafting = new Point16(i, j);
				Main.NewText("Portable access successfully set to: X=" + i + ", Y=" + j);
			}

			return false;
		}

		return storage != Point16.NegativeOne || crafting != Point16.NegativeOne;
	}

    public override bool AltFunctionUse(Player player) => true;

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI != Main.myPlayer) return base.UseItem(player);

		UISystem system = ModContent.GetInstance<UISystem>();

		if (Main.mouseLeft)
		{
			player.itemAnimation = 8;

			system.AccessState.Open(true);
		}
		else
		{
			if (system.ItemUI.CurrentState == system.AccessState)
			{
				int selected = system.AccessState.Selected;
				system.AccessState.Close(true);

				Point16 access;
				access = selected == AccessSelector.SELECTION_STORAGE  ? storage : Point16.NegativeOne;
				access = selected == AccessSelector.SELECTION_CRAFTING ? crafting : access;

				if (access != Point16.NegativeOne)
				{
					Tile tile = Main.tile[access.X, access.Y];
					if (!tile.HasTile) return false;

					StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();

					modPlayer.OpenStorage(access, true);

					return true;
				}

				return false;
			}
		}

		return base.UseItem(player);
	}

	}
	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Locator>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Emerald, 7)
			.AddIngredient(ItemID.Wire, 5)
			.Register();
	}

    public override void SaveData(TagCompound tag)
    {
        base.SaveData(tag);

		TagCompound storageTag = new TagCompound()
		{
			{ "X", storage.X },
			{ "Y", storage.Y }
		};

		TagCompound craftingTag = new TagCompound()
		{
			{ "X", crafting.X },
			{ "Y", crafting.Y }
		};

		tag.Set("Storage",  storageTag);
		tag.Set("Crafting", craftingTag);
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

		TagCompound storageTag = tag.GetCompound("Storage");
		storage = new Point16(storageTag.GetShort("X"), storageTag.GetShort("Y"));

		TagCompound craftingTag = tag.GetCompound("Crafting");
		crafting = new Point16(craftingTag.GetShort("X"), craftingTag.GetShort("Y"));
    }
}
