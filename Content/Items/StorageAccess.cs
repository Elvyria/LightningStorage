namespace MagicStorage.Content.Items
{
    public class StorageAccess : ModItem
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
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 67, copper: 50);
            Item.createTile = ModContent.TileType<Tiles.StorageAccess>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Items.StorageComponent>()
                .AddIngredient(ItemID.Diamond, 1)
                .AddIngredient(ItemID.Topaz, 7)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
