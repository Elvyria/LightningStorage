using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage.Items
{
	public class UpgradeHellstone : ModItem
	{

		public override void SetDefaults()
		{
			item.width = 12;
			item.height = 12;
			item.maxStack = 99;
			item.rare = 2;
			item.value = Item.sellPrice(0, 0, 40, 0);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HellstoneBar, 10);
			recipe.AddIngredient(ItemID.Topaz);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
