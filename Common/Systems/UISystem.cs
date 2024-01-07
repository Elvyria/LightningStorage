using Microsoft.Xna.Framework;

using LightningStorage.Common.UI;
using LightningStorage.Common.UI.States;

namespace LightningStorage.Common.Systems;

public class UISystem : ModSystem
{
#nullable disable
	internal HashSet<IInput> inputs;

	internal UserInterface UI;
	internal StorageAccessUI StorageUI;
	internal CraftingAccessUI CraftingUI;

	internal UserInterface ItemUI;
	internal PortableAccessUI AccessState;
#nullable restore

	private GameTime? _lastUpdateUiGameTime;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			inputs = new HashSet<IInput>(2);

			UI = new UserInterface();

			StorageUI = new StorageAccessUI();
			CraftingUI = new CraftingAccessUI();

			ItemUI = new UserInterface();
			AccessState = new PortableAccessUI();
		}
	}

	public override void Unload()
	{
		inputs = null;

		UI = null;
		StorageUI = null;
		CraftingUI = null;

		ItemUI = null;
		AccessState = null;

		_lastUpdateUiGameTime = null;
	}

	public override void PreSaveAndQuit()
	{
		UI.SetState(null);
		ItemUI.SetState(null);
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
		layers.Insert(invIndex + 1, new LegacyGameInterfaceLayer(
					"LightningStorage: StorageAccess",
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

		int wireIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
		layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
					"LightningStorage: Portable Access",
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
