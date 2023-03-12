namespace MagicStorage.Content.Items;

public class UpgradeDemonite : ModItem, Common.IStorageUpgrade
{
	public override void SetDefaults()
	{
		Item.width = 12;
		Item.height = 12;
		Item.maxStack = 99;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 32);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.Amethyst)
			.AddTile(TileID.Anvils)
			.Register();
	}

	public void Upgrade(int i, int j)
	{
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.Demonite);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.Default
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
