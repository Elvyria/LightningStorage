using Microsoft.Xna.Framework;


using MagicStorage.Common.Players;
using MagicStorage.Common.UI;

namespace MagicStorage.Common.Systems
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

        public override void Unload()
        {
            StorageUI = null;
            CraftingUI = null;
        }

		public override void PreSaveAndQuit() {
			StoragePlayer.LocalPlayer.CloseStorage();
		}

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (UI?.CurrentState != null)
            {
                UI.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int invIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (invIndex != -1)
            {
                layers.Insert(invIndex + 1, new LegacyGameInterfaceLayer(
                            "MagicStorage: StorageAccess",
                            delegate
                            {
                                if (_lastUpdateUiGameTime != null && UI?.CurrentState != null)
                                {
									Main.hidePlayerCraftingMenu = true;
                                    UI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                                }
                                return true;
                            },
                            InterfaceScaleType.UI));
            }
        }
    }
}
