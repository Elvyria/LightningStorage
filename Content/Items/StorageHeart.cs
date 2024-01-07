namespace LightningStorage.Content.Items;

public class StorageHeart : ModItem
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
		Item.value = Item.sellPrice(gold: 1, silver: 35);
		Item.createTile = ModContent.TileType<Tiles.StorageHeart>();
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Emerald, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
