using Terraria.DataStructures;

using MagicStorage.Content.TileEntities;

namespace MagicStorage.Content.Items;

public class StorageDeactivator : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 24;
		Item.height = 28;
		Item.useTurn = true;
		Item.autoReuse = true;
		Item.useAnimation = 15;
		Item.useTime = 15;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.tileBoost = 20;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 40);
	}

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer && player.itemAnimation > 0 && player.itemTime == 0 && player.controlUseItem)
		{
			int i = Player.tileTargetX;
			int j = Player.tileTargetY;
			if (Main.tile[i, j].TileFrameX % 36 == 18)
			{
				i--;
			}
			if (Main.tile[i, j].TileFrameY % 36 == 18)
			{
				j--;
			}
			Point16 point = new Point16(i, j);
			if (TileEntity.ByPosition.ContainsKey(point) && TileEntity.ByPosition[point] is TEStorageUnit storageUnit)
			{
				storageUnit.active = !storageUnit.active;
				string activeText = storageUnit.active ? "Activated" : "Deactivated" ;
				Main.NewText("Storage Unit has been " + activeText);
				if (storageUnit is TEStorageUnit)
				{
					((TEStorageUnit)storageUnit).UpdateTileFrame();
					storageUnit.GetHeart().ResetCompactStage();
				}
			}
		}
		return true;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.ActuationRod)
			.AddIngredient<Items.StorageComponent>()
			.AddTile(TileID.Anvils)
			.Register();
	}
}
