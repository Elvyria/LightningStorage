using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
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
			Item.useStyle = 1;
			Item.consumable = true;
			Item.rare = 1;
			Item.value = Item.sellPrice(0, 1, 16, 25);
			Item.createTile = ModContent.TileType<Components.CraftingAccess>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(null, "StorageComponent")
				.AddIngredient(ItemID.Diamond, 3)
				.AddIngredient(ItemID.Sapphire, 7)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
