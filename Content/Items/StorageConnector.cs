namespace MagicStorage.Content.Items
{
    public class StorageConnector : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 10);
            Item.createTile = ModContent.TileType<Tiles.StorageConnector>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(16)
                .AddRecipeGroup(RecipeGroupID.Wood, 16)
                .AddRecipeGroup(RecipeGroupID.IronBar)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
