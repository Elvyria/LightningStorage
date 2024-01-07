namespace LightningStorage.Content.Items;

public class UpgradeLuminite : ModItem, Common.IStorageUpgrade
{
	public override void SetDefaults()
	{
		Item.width = 12;
		Item.height = 12;
		Item.maxStack = 99;
		Item.rare = ItemRarityID.Red;
		Item.value = Item.sellPrice(gold: 1, silver: 50);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.LunarBar, 10)
			.AddIngredient(ItemID.FragmentSolar, 5)
			.AddIngredient(ItemID.FragmentVortex, 5)
			.AddIngredient(ItemID.FragmentNebula, 5)
			.AddIngredient(ItemID.FragmentStardust, 5)
			.AddIngredient(ItemID.Diamond)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
	}

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.Luminite);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.BlueChlorophyte
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
