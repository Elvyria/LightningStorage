﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagicStorage.Content.Items
{
    public class RemoteAccess : ModItem
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
            Item.rare = 1;
            Item.value = Item.sellPrice(0, 1, 72, 50);
            Item.createTile = ModContent.TileType<Tiles.RemoteAccess>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(null, "StorageComponent")
                .AddIngredient(ItemID.Diamond, 3)
                .AddIngredient(ItemID.Ruby, 7)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
