using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage
{
	public class MagicStorageSystem : ModSystem
	{

#pragma warning disable 0649
		internal UserInterface UI;
		internal StorageGUI StorageUI;
		internal CraftingGUI CraftingUI;
#pragma warning restore 0649

		private GameTime _lastUpdateUiGameTime;

        public override void Load()
        {
			if (!Main.dedServ)
			{
				UI = new UserInterface();

				StorageUI = new StorageGUI();
				CraftingUI = new CraftingGUI();
			}
        }

		public override void Unload()
		{
			StorageUI = null;
			CraftingUI = null;
		}

		public override void UpdateUI(GameTime gameTime) {
			_lastUpdateUiGameTime = gameTime;
			if (UI?.CurrentState != null) {
				UI.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int invIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (invIndex != -1)
			{
				layers.Insert(invIndex, new LegacyGameInterfaceLayer(
							"MagicStorage: StorageAccess",
							delegate
							{
								if (_lastUpdateUiGameTime != null && UI?.CurrentState != null) {
									UI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
								}

								return true;
							},
							InterfaceScaleType.UI));
			}
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Language.GetText("LangMisc.37") + " Chest",
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
			RecipeGroup.RegisterGroup("MagicStorage:AnyChest", group);

			group = new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Language.GetTextValue("Mods.MagicStorage.Common.SnowBiomeBlock"), ItemID.SnowBlock, ItemID.IceBlock, ItemID.PurpleIceBlock, ItemID.PinkIceBlock);
			RecipeGroup.RegisterGroup("MagicStorage:AnySnowBiomeBlock", group);
		}
	}
}

