using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Localization;

using LightningStorage.Common.Players;
using LightningStorage.Content.TileEntities;

namespace LightningStorage.Content.Items;

public class Locator : ModItem
{
	public Point16 location = Point16.NegativeOne;

	protected override bool CloneNewInstances => true;

	private const int useTime = 18;

	public override void SetDefaults()
	{
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = 1;
		Item.rare = ItemRarityID.Blue;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.useAnimation = useTime;
		Item.useTime = useTime;
		Item.value = Item.sellPrice(gold: 1);
	}

    public override bool AltFunctionUse(Player player) => true;

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2 && player.itemAnimation == 0)
		{
			(int i, int j) = StoragePlayer.MouseTile();

			Tile tile = Main.tile[i, j];

			return tile.HasTile && (tile.TileType == ModContent.TileType<Tiles.StorageHeart>() || tile.TileType == ModContent.TileType<Tiles.RemoteAccess>());
		}

		return false;
	}

    public override bool? UseItem(Player player)
    {
		if (player.itemAnimation != useTime) return false;

		(int i, int j) = StoragePlayer.MouseTile();

		Tile tile = Main.tile[i, j];

		(i, j) = tile.FrameOrigin(i, j);

		if (tile.TileType == ModContent.TileType<Tiles.StorageHeart>())
		{
			location = new Point16(i, j);
			Main.NewText("Locator successfully set to: X=" + i + ", Y=" + j);

			return true;
		}

		if (tile.TileType == ModContent.TileType<Tiles.RemoteAccess>())
		{
			TERemoteAccess ent = (TERemoteAccess) TileEntity.ByPosition[new Point16(i, j)];
			if (ent.Bind(location))
			{
				Main.NewText("Success !!");
				return true;
			}
		}

        return false;
    }

	public override void ModifyTooltips(List<TooltipLine> lines)
	{
		bool isSet = location != Point16.NegativeOne;
		for (int k = 0; k < lines.Count; k++)
		{
			if (isSet && lines[k].Mod == "Terraria" && lines[k].Name == "Tooltip0")
			{
				lines[k].Text = Language.GetTextValue("Mods.LightningStorage.Common.SetTo", location.X, location.Y);
			}
			else if (!isSet && lines[k].Mod == "Terraria" && lines[k].Name == "Tooltip1")
			{
				lines.RemoveAt(k);
				k--;
			}
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.GPS)
			.AddIngredient(ItemID.MeteoriteBar, 10)
			.AddIngredient(ItemID.Amber, 5)
			.AddTile(TileID.Anvils)
			.Register();
	}

	public override void SaveData(TagCompound tag)
	{
		tag.SetPoint16("Location", location);
	}

	public override void LoadData(TagCompound tag)
	{
		location = tag.GetPoint16("Location");
	}
}
