using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
	public class StorageComponent : ModItem
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
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.createTile = ModContent.TileType<Components.StorageComponent>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
