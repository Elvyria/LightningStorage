namespace MagicStorage.Content.Items;

public class UpgradeTerra : ModItem, Common.IStorageUpgrade
{
	public override void SetDefaults()
	{
		Item.width = 12;
		Item.height = 12;
		Item.maxStack = 99;
		Item.rare = ItemRarityID.Purple;
		Item.value = Item.sellPrice(gold: 10);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Ruby, 7)
			.AddTile(TileID.LunarCraftingStation)
			.Register();

		if (ModLoader.TryGetMod("CalamityMod", out var calamityMod))
		{
			CreateRecipe()
				.AddIngredient(calamityMod, "CosmiliteBar", 20)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageUnit.SetStyle(i, j, Tiles.StorageUnit.StyleID.Terra);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.Luminite
			&& tile.TileType == ModContent.TileType<Tiles.StorageUnit>();
	}
}
