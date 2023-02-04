using Terraria;
using Terraria.ID;

namespace MagicStorage.Items
{
	public class LocatorDisk : Locator
	{

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.maxStack = 1;
			Item.rare = 10;
			Item.value = Item.sellPrice(0, 5, 0, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.MartianConduitPlating, 25)
				.AddIngredient(ItemID.LunarBar, 5)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
