using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using LightningStorage.Common.Systems;

namespace LightningStorage.Common.UI.States;

class PortableAccessUI : UIState, ISwitchable
{
	public bool Initialized { get; private set; }
	public int  Selected    { get => selector.Selected; }

	public const int SELECTION_STORAGE  = 0;
	public const int SELECTION_CRAFTING = 1;

	[AllowNull]
	private UISelector selector;

	private bool opening;
	private bool closing;

    public override void OnInitialize()
    {
		selector = new UISelector(new Asset<Texture2D>[]
				{
					ModContent.Request<Texture2D>("LightningStorage/Content/Items/StorageAccess"),
					ModContent.Request<Texture2D>("LightningStorage/Content/Items/CraftingAccess"),
				},
				new Vector2[]
				{
					new Vector2(-35f, -35f),
					new Vector2(35f, -35f)
				});

		Append(selector);

		Initialized = true;
    }

	public void Open(bool animate = false)
	{
		UISystem system = ModContent.GetInstance<UISystem>();
		if (system.ItemUI.CurrentState == this) return;

		if (!Initialized) Initialize();

		selector.Open(animate);

		opening = true;
		closing = false;

		system.ItemUI.SetState(this);
	}

	public void Close(bool animate = false)
	{
		UISystem system = ModContent.GetInstance<UISystem>();
		if (system.ItemUI.CurrentState == null) return;

		if (animate)
		{
			opening = false;
			closing = true;

			selector.Close(true);
		}
		else
		{
			system.ItemUI.SetState(null);
		}
	}

	public override void OnDeactivate()
	{
		opening = false;
		closing = false;
	}

    public override void Update(GameTime gameTime)
    {
		if (closing && !selector.Closing)
		{
			Close();
			return;
		}

		base.Update(gameTime);

		if (opening && !selector.Opening)
		{
			opening = false;
		}

		Vector2 position = Main.LocalPlayer.Center - Main.Camera.UnscaledPosition;

		selector.Left.Pixels = position.X;
		selector.Top.Pixels = position.Y;
		selector.Recalculate();
    }
}
