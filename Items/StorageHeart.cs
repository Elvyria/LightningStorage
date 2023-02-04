using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
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
			Item.useStyle = 1;
			Item.consumable = true;
			Item.rare = 1;
			Item.value = Item.sellPrice(0, 1, 35, 0);
			Item.createTile = ModContent.TileType<Components.StorageHeart>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(null, "StorageComponent")
				.AddIngredient(ItemID.Diamond, 3)
				.AddIngredient(ItemID.Emerald, 7)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
