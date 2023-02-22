namespace MagicStorage.Content.Items;

public class UpgradeHellstone : ModItem
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
			.AddIngredient(ItemID.Topaz)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
