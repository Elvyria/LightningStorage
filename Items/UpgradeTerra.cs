using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
	public class UpgradeTerra : ModItem
	{

		public override void SetDefaults()
		{
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = 11;
			Item.value = Item.sellPrice(0, 10, 0, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Diamond, 3)
				.AddIngredient(ItemID.Ruby, 7)
				.AddTile(TileID.LunarCraftingStation)
				.Register();

			if (ModLoader.TryGetMod("CalamityMod", out var calamityMod))
			{
				CreateRecipe()
					.AddIngredient(calamityMod, "CosmiliteBar", 20)
					.AddTile(TileID.LunarCraftingStation)
					.Register();
			}
		}
	}
}
