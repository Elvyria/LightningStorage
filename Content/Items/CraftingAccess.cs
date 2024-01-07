namespace LightningStorage.Content.Items;

public class CraftingAccess : ModItem
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
		Item.value = Item.sellPrice(gold: 1, silver: 16, copper: 25);
		Item.createTile = ModContent.TileType<Tiles.CraftingAccess>();
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Items.StorageComponent>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Sapphire, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

public class CraftingAccessDemonite : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Demonite;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessCrimtane : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Crimtane;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessHellstone : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Green;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Hellstone;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessHallowed : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.LightRed;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Hallowed;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessBlueChlorophyte : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Lime;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.BlueChlorophyte;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessLuminite : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Red;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Luminite;
	}

	public override void AddRecipes() {}
}

public class CraftingAccessTerra : CraftingAccess
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Purple;
		Item.placeStyle = Tiles.CraftingAccess.StyleID.Terra;
	}

	public override void AddRecipes() {}
}
