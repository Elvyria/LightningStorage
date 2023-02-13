using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class UpgradeLuminite : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.rare = 10;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LunarBar, 10)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddIngredient(ItemID.Ruby)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
