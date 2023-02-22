namespace MagicStorage.Content.Items;

public class UpgradeHallowed : ModItem
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
			.AddIngredient(ItemID.Sapphire)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
