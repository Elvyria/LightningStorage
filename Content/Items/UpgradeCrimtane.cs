
namespace MagicStorage.Content.Items
{
    public class UpgradeCrimtane : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 32);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 10)
                .AddIngredient(ItemID.Amethyst)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
