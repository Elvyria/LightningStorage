using Terraria;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
	public class StorageUnitDemonite : ModItem
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
			Item.useStyle = 1;
			Item.consumable = true;
			Item.rare = 1;
			Item.value = Item.sellPrice(0, 0, 32, 0);
			Item.createTile = ModContent.TileType<Components.StorageUnit>();
			Item.placeStyle = 1;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Items.StorageUnit>())
				.AddIngredient(ModContent.ItemType<Items.UpgradeDemonite>())
				.Register();
		}
	}
}
