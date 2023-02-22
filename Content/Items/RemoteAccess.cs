namespace MagicStorage.Content.Items;

public class RemoteAccess : ModItem
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
		Item.value = Item.sellPrice(gold: 1, silver: 72, copper: 50);
		Item.createTile = ModContent.TileType<Tiles.RemoteAccess>();
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Ruby, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
