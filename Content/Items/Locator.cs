using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Localization;

namespace MagicStorage.Content.Items;

public class Locator : ModItem
{
	public Point16 location = Point16.NegativeOne;

	protected override bool CloneNewInstances
	{
		get
		{
			return true;
		}
	}

	public override void SetDefaults()
	{
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = 1;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 1);
	}

	public override void ModifyTooltips(List<TooltipLine> lines)
	{
		bool isSet = location.X >= 0 && location.Y >= 0;
		for (int k = 0; k < lines.Count; k++)
		{
			if (isSet && lines[k].Mod == "Terraria" && lines[k].Name == "Tooltip0")
			{
				lines[k].Text = Language.GetTextValue("Mods.MagicStorage.Common.SetTo", location.X, location.Y);
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
			.AddIngredient(ItemID.MeteoriteBar, 10)
			.AddIngredient(ItemID.Amber, 5)
			.AddTile(TileID.Anvils)
			.Register();
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Set("X", location.X);
		tag.Set("Y", location.Y);
	}

	public override void LoadData(TagCompound tag)
	{
		location = new Point16(tag.GetShort("X"), tag.GetShort("Y"));
	}
}
