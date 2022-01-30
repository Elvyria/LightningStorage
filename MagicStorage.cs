using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.Localization;

namespace MagicStorage
{
	public class MagicStorage : Mod
	{
		public static MagicStorage Instance;
		public static Mod bluemagicMod;
		public static Mod legendMod;

		internal UserInterface UI;
		internal StorageGUI StorageUI;
		internal CraftingGUI CraftingUI;

		private GameTime _lastUpdateUiGameTime;

		public static readonly Version requiredVersion = new Version(0, 11);

		public override void Load()
		{
			if (ModLoader.version < requiredVersion)
			{
				throw new Exception("Magic storage requires a tModLoader version of at least " + requiredVersion);
			}

			Instance = this;
			legendMod = ModLoader.GetMod("LegendOfTerraria3");
			bluemagicMod = ModLoader.GetMod("Bluemagic");

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

			Instance = null;
			bluemagicMod = null;
			legendMod = null;
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
			if (bluemagicMod != null)
			{
				group.ValidItems.Add(bluemagicMod.ItemType("DarkBlueIce"));
			}
			RecipeGroup.RegisterGroup("MagicStorage:AnySnowBiomeBlock", group);

			group = new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Diamond), ItemID.Diamond, ItemType("ShadowDiamond"));
			if (legendMod != null)
			{
				group.ValidItems.Add(legendMod.ItemType("GemChrysoberyl"));
				group.ValidItems.Add(legendMod.ItemType("GemAlexandrite"));
			}

			RecipeGroup.RegisterGroup("MagicStorage:AnyDiamond", group);

			if (legendMod != null)
			{
				RecipeGroup.RegisterGroup("MagicStorage:AnyAmethyst",
						new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Amethyst), ItemID.Amethyst, legendMod.ItemType("GemOnyx"), legendMod.ItemType("GemSpinel")));
				RecipeGroup.RegisterGroup("MagicStorage:AnyTopaz",
						new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Topaz), ItemID.Topaz, legendMod.ItemType("GemGarnet")));
				RecipeGroup.RegisterGroup("MagicStorage:AnySapphire",
						new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Sapphire), ItemID.Sapphire, legendMod.ItemType("GemCharoite")));
				RecipeGroup.RegisterGroup("MagicStorage:AnyEmerald",
						new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Emerald), legendMod.ItemType("GemPeridot")));
				RecipeGroup.RegisterGroup("MagicStorage:AnyRuby",
						new RecipeGroup(() => Language.GetText("LangMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Ruby), ItemID.Ruby, legendMod.ItemType("GemOpal")));
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetHelper.HandlePacket(reader, whoAmI);
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
								if (_lastUpdateUiGameTime != null && UI?.CurrentState != null)
								{
									UI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
								}

								return true;
							},
							InterfaceScaleType.UI));
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			_lastUpdateUiGameTime = gameTime;
			if (UI?.CurrentState != null) {
				UI.Update(gameTime);
			}
		}
	}
}

