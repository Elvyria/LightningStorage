namespace MagicStorage.Content.Items;

public class StorageUnitHallowed : ModItem
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
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 1);
		Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
		Item.placeStyle = Tiles.StorageUnit.StyleID.Hallowed;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageUnitHellstone>()
			.AddIngredient<Items.UpgradeHallowed>()
			.Register();
	}
}
