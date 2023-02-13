using Terraria;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class SnowBiomeEmulator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.rare = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("MagicStorage:AnySnowBiomeBlock", 300)
                .AddTile(null, "CraftingAccess")
                .Register();
        }
    }
}
