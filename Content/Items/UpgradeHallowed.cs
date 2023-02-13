using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class UpgradeHallowed : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.rare = 4;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 10)
                .AddIngredient(ItemID.SoulofFright)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ItemID.Sapphire)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
