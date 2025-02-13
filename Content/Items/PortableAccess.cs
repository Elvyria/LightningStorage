using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using LightningStorage.Common.Players;
using LightningStorage.Common.UI.States;
using LightningStorage.Common.Systems;

namespace LightningStorage.Content.Items;

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
			(int i, int j) = StoragePlayer.MouseTile();

			Tile tile = Main.tile[i, j];

			(i, j) = tile.FrameOrigin(i, j);

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
				access = selected == PortableAccessUI.SELECTION_STORAGE  ? storage : Point16.NegativeOne;
				access = selected == PortableAccessUI.SELECTION_CRAFTING ? crafting : access;

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

		tag.SetPoint16("Storage",  storage);
		tag.SetPoint16("Crafting", crafting);
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

		storage  = tag.GetPoint16("Storage");
		crafting = tag.GetPoint16("Crafting");
    }
}
