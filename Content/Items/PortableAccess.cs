using Terraria.DataStructures;
using Terraria.ModLoader.IO;

using MagicStorage.Common.Players;

namespace MagicStorage.Content.Items;

public class PortableAccess : ModItem
{
	public Point16 storage = Point16.NegativeOne;
	public Point16 crafting = Point16.NegativeOne;

	public override void SetDefaults()
	{
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = 1;
		Item.rare = ItemRarityID.Purple;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.useAnimation = 28;
		Item.useTime = 28;
		Item.value = Item.sellPrice(gold: 10);
	}

    public override bool CanUseItem(Player player)
    {
		return storage != Point16.NegativeOne || crafting != Point16.NegativeOne;
    }

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer)
		{
			Point16 access = storage;

			if (access != Point16.NegativeOne)
			{
				Tile tile = Main.tile[access.X, access.Y];
				if (!tile.HasTile) return false;

				StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();

				modPlayer.OpenStorage(access, true);

				return true;
			}
		}

		return false;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<Locator>()
			.AddIngredient(ItemID.Diamond, 3)
			.AddIngredient(ItemID.Emerald, 7)
			.AddIngredient(ItemID.Wire, 5)
			.Register();
	}

    public override void SaveData(TagCompound tag)
    {
        base.SaveData(tag);

		TagCompound storageTag = new TagCompound()
		{
			{ "X", storage.X },
			{ "Y", storage.Y }
		};

		TagCompound craftingTag = new TagCompound()
		{
			{ "X", crafting.X },
			{ "Y", crafting.Y }
		};

		tag.Set("Storage",  storageTag);
		tag.Set("Crafting", craftingTag);
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

    }
}
