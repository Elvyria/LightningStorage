using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage.Items
{
	public class UpgradeTerra : ModItem
	{

		public override void SetDefaults()
		{
			item.width = 12;
			item.height = 12;
			item.maxStack = 99;
			item.rare = 11;
			item.value = Item.sellPrice(0, 10, 0, 0);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "RadiantJewel");
			recipe.AddRecipeGroup("MagicStorage:AnyDiamond");
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();

			Mod otherMod = MagicStorage.bluemagicMod;
			if (otherMod != null)
			{
				recipe = new ModRecipe(mod);
				recipe.AddIngredient(otherMod, "InfinityCrystal");
				recipe.AddRecipeGroup("MagicStorage:AnyDiamond");
				recipe.AddTile(otherMod, "PuriumAnvil");
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

			otherMod = ModLoader.GetMod("CalamityMod");
			if (otherMod != null)
			{
				recipe = new ModRecipe(mod);
				recipe.AddIngredient(otherMod, "CosmiliteBar", 20);
				recipe.AddRecipeGroup("MagicStorage:AnyDiamond");
				recipe.AddTile(TileID.LunarCraftingStation);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}
}
