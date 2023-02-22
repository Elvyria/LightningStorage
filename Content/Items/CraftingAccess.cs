namespace MagicStorage.Content.Items;

public class CraftingAccess : ModItem
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
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 1, silver: 16, copper: 25);
		Item.createTile = ModContent.TileType<Tiles.CraftingAccess>();
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Sapphire, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
