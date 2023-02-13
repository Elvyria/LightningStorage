using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class StorageUnit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = 1;
            Item.consumable = true;
            Item.rare = 0;
            Item.value = Item.sellPrice(0, 0, 6, 0);
            Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.StorageComponent>())
                .AddRecipeGroup("MagicStorage:AnyChest")
                .AddIngredient(ItemID.SilverBar, 10)
                .AddTile(TileID.WorkBenches)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.StorageComponent>())
                .AddRecipeGroup("MagicStorage:AnyChest")
                .AddIngredient(ItemID.TungstenBar, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
