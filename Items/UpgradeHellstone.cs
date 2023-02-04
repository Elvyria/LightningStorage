using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
	public class UpgradeHellstone : ModItem
	{

		public override void SetDefaults()
		{
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = 2;
			Item.value = Item.sellPrice(0, 0, 40, 0);
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
}
