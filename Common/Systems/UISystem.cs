using Microsoft.Xna.Framework;

using MagicStorage.Common.Players;
using MagicStorage.Common.UI;
using MagicStorage.Common.UI.States;

namespace MagicStorage.Common.Systems;

public class UISystem : ModSystem
{
	internal readonly HashSet<IInput> inputs = new HashSet<IInput>(2);

#pragma warning disable 0649
	internal UserInterface UI;
	internal StorageGUI StorageUI;
	internal CraftingGUI CraftingUI;
#pragma warning restore 0649

	internal UserInterface ItemUI;
	internal AccessSelector AccessState;

	private GameTime _lastUpdateUiGameTime;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			UI = new UserInterface();

			StorageUI = new StorageGUI();
			CraftingUI = new CraftingGUI();

			ItemUI = new UserInterface();
			AccessState = new AccessSelector();
		}
	}

	public override void Unload()
	{
		StorageUI = null;
		CraftingUI = null;

		ItemUI = null;
		AccessState = null;
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

		if (ItemUI?.CurrentState != null)
		{
			ItemUI.Update(gameTime);
		}
	}

    public override void PostUpdateInput()
    {
		foreach (IInput input in inputs)
		{
			input.UpdateInput();
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

		int wireIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
		layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
					"MagicStorage: Portable Access",
					delegate
					{
						if (_lastUpdateUiGameTime != null && ItemUI?.CurrentState != null)
						{
							ItemUI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
						}
						return true;
					},
					InterfaceScaleType.UI));
	}
}
