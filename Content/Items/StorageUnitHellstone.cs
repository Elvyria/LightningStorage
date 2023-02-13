using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class StorageUnitHellstone : ModItem
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
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 50);
            Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
            Item.placeStyle = 3;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Items.StorageUnitDemonite>()
                .AddIngredient<Items.UpgradeHellstone>()
                .Register();

            CreateRecipe()
                .AddIngredient<Items.StorageUnitCrimtane>()
                .AddIngredient<Items.UpgradeHellstone>()
                .Register();
        }
    }
}
