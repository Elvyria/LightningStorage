namespace MagicStorage.Content.Items;

public class StorageUnitBlueChlorophyte : ModItem
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
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 1, silver: 60);
        Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
        Item.placeStyle = Tiles.StorageUnit.StyleID.BlueChlorophyte;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<Items.StorageUnitHallowed>()
            .AddIngredient<Items.UpgradeBlueChlorophyte>()
            .Register();
    }
}
