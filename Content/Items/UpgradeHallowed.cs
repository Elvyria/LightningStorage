namespace MagicStorage.Content.Items;

public class UpgradeHallowed : ModItem, Common.IStorageUpgrade
{
	public override void SetDefaults()
	{
		Item.width = 12;
		Item.height = 12;
		Item.maxStack = 99;
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(silver: 40);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HallowedBar, 10)
			.AddIngredient(ItemID.SoulofFright)
			.AddIngredient(ItemID.SoulofMight)
			.AddIngredient(ItemID.SoulofSight)
			.AddIngredient(ItemID.Topaz)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}

	public void Upgrade(int i, int j) {
		Content.Tiles.StorageComponent.SetStyle(i, j, Tiles.StorageUnit.StyleID.Hallowed);
	}

	public bool CanUpgrade(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = tile.TileFrameY / 36;

		return style == Tiles.StorageUnit.StyleID.Hellstone
			&& ModContent.GetModTile(tile.TileType) is Common.IUpgradeable;
	}
}
