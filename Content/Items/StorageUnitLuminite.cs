namespace MagicStorage.Content.Items;

public class StorageUnitLuminite : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 26;
		Item.height = 26;
		Item.maxStack = 99;
		Item.useTurn = true;
		Item.autoReuse = true;
		Item.useAnimation = 15;
		Item.useTime = 10;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.consumable = true;
		Item.rare = ItemRarityID.Red;
		Item.value = Item.sellPrice(gold: 2, silver: 50);
		Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
		Item.placeStyle = 6;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageUnitBlueChlorophyte>()
			.AddIngredient<Items.UpgradeLuminite>()
			.Register();
	}
}
