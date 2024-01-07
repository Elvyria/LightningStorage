namespace LightningStorage.Content.Items;

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
		Item.useStyle = ItemUseStyleID.Swing;
		Item.consumable = true;
		Item.rare = ItemRarityID.White;
		Item.value = Item.sellPrice(silver: 6);
		Item.createTile = ModContent.TileType<Tiles.StorageUnit>();
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddRecipeGroup("LightningStorage:AnyChest")
			.AddIngredient(ItemID.SilverBar, 10)
			.AddTile(TileID.WorkBenches)
			.Register();

		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddRecipeGroup("LightningStorage:AnyChest")
			.AddIngredient(ItemID.TungstenBar, 10)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

public class StorageUnitTiny : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.placeStyle = Tiles.StorageUnit.StyleID.Tiny;
	}
}

public class StorageUnitDemonite : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 32);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Demonite;
	}
}

public class StorageUnitCrimtane : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 32);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Crimtane;
	}
}

public class StorageUnitHellstone : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 50);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Hellstone;
	}
}

public class StorageUnitHallowed : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 1);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Hallowed;
	}
}

public class StorageUnitBlueChlorophyte : StorageUnit
{
    public override void SetDefaults()
    {
		base.SetDefaults();
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 1, silver: 60);
		Item.placeStyle = Tiles.StorageUnit.StyleID.BlueChlorophyte;
	}
}

public class StorageUnitLuminite : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Red;
		Item.value = Item.sellPrice(gold: 2, silver: 50);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Luminite;
	}
}

public class StorageUnitTerra : StorageUnit
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Purple;
		Item.value = Item.sellPrice(silver: 12);
		Item.placeStyle = Tiles.StorageUnit.StyleID.Terra;
	}
}
