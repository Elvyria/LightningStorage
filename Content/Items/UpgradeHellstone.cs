namespace LightningStorage.Content.Items;

public class UpgradeHellstone : ModItem, Common.IStorageUpgrade
{
	public override void SetDefaults()
	{
		Item.width = 12;
		Item.height = 12;
		Item.maxStack = 99;
		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 40);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HellstoneBar, 10)
			.AddIngredient(ItemID.Amber)
			.AddTile(TileID.Anvils)
			.Register();
	}

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.Hellstone);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return (style == Tiles.StorageUnit.StyleID.Demonite || style == Tiles.StorageUnit.StyleID.Crimtane)
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
