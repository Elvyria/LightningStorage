namespace LightningStorage.Content.Items;

public class UpgradeBlueChlorophyte : ModItem, Common.IStorageUpgrade
{
    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 12;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 5)
            .AddIngredient(ItemID.SpectreBar, 5)
            .AddIngredient(ItemID.BeetleHusk, 2)
            .AddIngredient(ItemID.Sapphire)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.BlueChlorophyte);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.Hallowed
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
