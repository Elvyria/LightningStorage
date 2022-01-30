using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage.Items
{
	public class UpgradeHallowed : ModItem
	{

		public override void SetDefaults()
		{
			item.width = 12;
			item.height = 12;
			item.maxStack = 99;
			item.rare = 4;
			item.value = Item.sellPrice(0, 0, 40, 0);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 10);
			recipe.AddIngredient(ItemID.SoulofFright);
			recipe.AddIngredient(ItemID.SoulofMight);
			recipe.AddIngredient(ItemID.SoulofSight);
			recipe.AddIngredient(ItemID.Sapphire);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
