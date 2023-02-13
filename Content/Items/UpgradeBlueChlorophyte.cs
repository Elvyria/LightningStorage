using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class UpgradeBlueChlorophyte : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShroomiteBar, 5)
                .AddIngredient(ItemID.SpectreBar, 5)
                .AddIngredient(ItemID.BeetleHusk, 2)
                .AddIngredient(ItemID.Emerald)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
