using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class UpgradeTerra : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 10);
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
