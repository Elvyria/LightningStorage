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
			.AddIngredient(ItemID.BrokenHeroSword)
			.AddIngredient(ItemID.Emerald)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
	}

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.Terra);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.Luminite
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
