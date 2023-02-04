using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Items
{
	public class UpgradeDemonite : ModItem
	{

		public override void SetDefaults()
		{
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = 1;
			Item.value = Item.sellPrice(0, 0, 32, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DemoniteBar, 10)
				.AddIngredient(ItemID.Amethyst)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
