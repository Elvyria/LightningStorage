using Terraria.Localization;

namespace LightningStorage.Common.Systems;

public class RecipeSystem : ModSystem
{
	public override void PostAddRecipes()
	{
		var logger = LightningStorage.Instance.Logger;

		foreach (Recipe recipe in Main.recipe)
		{
			if (recipe.Mod is null) continue;

			if (recipe.createItem.IsAir)
			{
				logger.WarnFormat("{0}: `{1}` recipe creates item that counts as air, this is not supported", recipe.Mod, recipe.createItem.Name);
			}

			foreach (Item item in recipe.requiredItem)
			{
				if (item.type <= 0)
					logger.WarnFormat("{0}: `{1}` recipe requires item with type {2} is detected", recipe.Mod, recipe.createItem.Name, item.type);

				if (item.stack <= 0)
					logger.WarnFormat("{0}: `{1}` recipe requires item with stack size {2}, this is not supported", recipe.Mod, recipe.createItem.Name, item.stack);
			}
		}
	}

	public override void AddRecipeGroups()
	{
		RecipeGroup group;

		group = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " Chest",
				ItemID.Chest,
				ItemID.GoldChest,
				ItemID.ShadowChest,
				ItemID.EbonwoodChest,
				ItemID.RichMahoganyChest,
				ItemID.PearlwoodChest,
				ItemID.IvyChest,
				ItemID.IceChest,
				ItemID.LivingWoodChest,
				ItemID.SkywareChest,
				ItemID.ShadewoodChest,
				ItemID.WebCoveredChest,
				ItemID.LihzahrdChest,
				ItemID.WaterChest,
				ItemID.JungleChest,
				ItemID.CorruptionChest,
				ItemID.CrimsonChest,
				ItemID.HallowedChest,
				ItemID.FrozenChest,
				ItemID.DynastyChest,
				ItemID.HoneyChest,
				ItemID.SteampunkChest,
				ItemID.PalmWoodChest,
				ItemID.MushroomChest,
				ItemID.BorealWoodChest,
				ItemID.SlimeChest,
				ItemID.GreenDungeonChest,
				ItemID.PinkDungeonChest,
				ItemID.BlueDungeonChest,
				ItemID.BoneChest,
				ItemID.CactusChest,
				ItemID.FleshChest,
				ItemID.ObsidianChest,
				ItemID.PumpkinChest,
				ItemID.SpookyChest,
				ItemID.GlassChest,
				ItemID.MartianChest,
				ItemID.GraniteChest,
				ItemID.MeteoriteChest,
				ItemID.MarbleChest);
		RecipeGroup.RegisterGroup("LightningStorage:AnyChest", group);
	}
}

