using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.UI;
using Terraria.ModLoader;

namespace MagicStorage
{
	public class UISystem : ModSystem
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

		// public override void OnWorldLoad() {
			// StorageUI.Initialize();
			// CraftingUI.Initialize();
		// }

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
	}
}
