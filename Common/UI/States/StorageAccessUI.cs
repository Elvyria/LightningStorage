using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.GameInput;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

using MagicStorage.Common.Systems;
using MagicStorage.Common.Players;
using MagicStorage.Common.Sorting;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Common.UI.States;

class StorageAccessUI : UIState, ISwitchable
{
#nullable disable
	private bool isMouseHovering;

	private const float inventoryScale = 0.8f;

	private const int padding = 4;
	private const int columns = 10;

	const float panelLeft = 20f;

	private float easeTimer = 0;
	public bool opening = false;
	public bool closing = false;

	private int stackSplit = 0;
	private int stackDelay = 7;
	private int stackCounter = 0;

	private Player player;
	private TEStorageHeart heart;
	private List<Item> items;

	private UIPanel panel;

	private UISlotZone slotZone;

	private UIText capacityText;

	private UISearchBar searchBar;
	private UISearchBar searchBar2;

	private UIButtonChoice sortButtons;
	private UIButtonChoice filterButtons;

	private IComparer<Item> sortMode = SortMode.Default;
	private IFilter<Item> filterMode = FilterMode.All;
	private string nameFilter = string.Empty;
	private string modFilter = string.Empty;
#nullable restore

	public override void OnInitialize()
	{
		float slotWidth = TextureAssets.InventoryBack9.Width() * inventoryScale;
		float slotHeight = TextureAssets.InventoryBack9.Height() * inventoryScale;

		float panelTop = Main.instance.invBottom + 60f;
		const float pannelBottom = 10f;

		panel = new UIPanel();

		float innerPanelLeft = panelLeft + panel.PaddingLeft;
		float innerPanelWidth = columns * (slotWidth + padding) + 20f + padding;
		float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
		float panelHeight = Main.screenHeight / Main.UIScale - panelTop - pannelBottom;

		panel.Left.Pixels = (int)(opening ? -panelWidth : panelLeft);
		panel.Top.Pixels = panelTop;
		panel.Width.Pixels = panelWidth;
		panel.Height.Pixels = panelHeight;
		panel.Recalculate();

		UIElement topBar = new UIElement();
		topBar.Width.Percent = 1f;
		topBar.Height.Pixels = 32f;
		panel.Append(topBar);

		sortButtons = new UIButtonChoice(new Asset<Texture2D>[]
				{
					TextureAssets.InventorySort[0],
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortID"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortName"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortNumber")
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.Common.SortDefault"),
					Language.GetText("Mods.MagicStorage.Common.SortID"),
					Language.GetText("Mods.MagicStorage.Common.SortName"),
					Language.GetText("Mods.MagicStorage.Common.SortStack")
				});

		sortButtons.OnLeftClick += (_, _) =>
		{
			IComparer<Item> newSort = SortMode.from(sortButtons.choice);
			if (newSort != sortMode)
			{
				sortMode = newSort;
				RefreshItems();
			}

			SoundEngine.PlaySound(SoundID.MenuTick);
		};

		topBar.Append(sortButtons);

		UITextPanel<LocalizedText> depositButton = new UITextPanel<LocalizedText>(Language.GetText("Mods.MagicStorage.Common.DepositAll"), 1f);
		depositButton.Left.Pixels = sortButtons.GetDimensions().Width + 2 * padding;
		depositButton.Width.Pixels = 128f;
		depositButton.Height.Set(-2 * padding, 1f);
		depositButton.PaddingTop = 8f;
		depositButton.PaddingBottom = 8f;
		depositButton.OnLeftClick += ClickDeposit;
		depositButton.OnMouseOver += (_, _) => depositButton.BackgroundColor = StateColor.lightBlue;
		depositButton.OnMouseOut  += (_, _) => depositButton.BackgroundColor = StateColor.blue;

		topBar.Append(depositButton);

		float depositButtonRight = sortButtons.GetDimensions().Width + 2 * padding + depositButton.GetDimensions().Width;
		searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchName"));
		searchBar.Left.Pixels = depositButtonRight + padding;
		searchBar.Width.Set(-depositButtonRight - 2 * padding, 1f);
		searchBar.Height.Percent = 1f;

		topBar.Append(searchBar);

		UIElement topBar2 = new UIElement();
		topBar2.Width.Percent = 1f;
		topBar2.Height.Pixels = 32f;
		topBar2.Top.Pixels = 36f;

		panel.Append(topBar2);

		filterButtons = new UIButtonChoice(new Asset<Texture2D>[]
				{
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterAll"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterMelee"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterPickaxe"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterArmor"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterPotion"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterTile"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/FilterMisc"),
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.Common.FilterAll"),
					Language.GetText("Mods.MagicStorage.Common.FilterWeapons"),
					Language.GetText("Mods.MagicStorage.Common.FilterTools"),
					Language.GetText("Mods.MagicStorage.Common.FilterEquips"),
					Language.GetText("Mods.MagicStorage.Common.FilterPotions"),
					Language.GetText("Mods.MagicStorage.Common.FilterTiles"),
					Language.GetText("Mods.MagicStorage.Common.FilterMisc")
				});

		filterButtons.OnLeftClick += (_, _) =>
		{
			IFilter<Item> newFilter = FilterMode.from(filterButtons.choice);
			if (newFilter != filterMode)
			{
				filterMode = newFilter;
				RefreshItems();
			}

			SoundEngine.PlaySound(SoundID.MenuTick);
		};

		topBar2.Append(filterButtons);

		searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchMod"));
		searchBar2.Left.Pixels = depositButtonRight + padding;
		searchBar2.Width.Set(-depositButtonRight - 2 * padding, 1f);
		searchBar2.Height.Percent = 1f;

		topBar2.Append(searchBar2);

		slotZone = new UISlotZone(GetItem, inventoryScale);
		slotZone.Width.Percent = 1f;
		slotZone.Top.Pixels = 86f;
		slotZone.Height.Set(-116f, 1f);
		slotZone.OnLeftMouseDown += PressSlotZone;

		panel.Append(slotZone);

		UIScrollableBar scrollbar = new UIScrollableBar();
		scrollbar.Left.Pixels = -30f;
		slotZone.Scrollbar = scrollbar;

		panel.Append(scrollbar);

		capacityText = new UIText(string.Empty);
		capacityText.Top.Set(-32f, 1f);
		capacityText.Left.Pixels = 6f;
		capacityText.Height.Pixels = 32f;
		panel.Append(capacityText);

		float slotZoneHeight = capacityText.GetDimensions().Y - (filterButtons.GetDimensions().Y + filterButtons.GetDimensions().Height);
		slotZone.SetDimensions(columns, (int)(slotZoneHeight / (slotHeight + slotZone.padding)));

		Append(panel);
	}

	private bool AnimationOpen()
	{
		panel.Left.Pixels = Animation.Ease(-panel.Width.Pixels, panelLeft, 0.2f, ref easeTimer);

		return Math.Abs(panel.Left.Pixels - panelLeft) > 0.1;
	}

	private bool AnimationClose()
	{
		panel.Left.Pixels = Animation.Ease(panelLeft, -panel.Width.Pixels, 0.2f, ref easeTimer);

		return Math.Abs(panel.Left.Pixels - (-panel.Width.Pixels)) > 0.1;
	}

	public override void OnActivate()
	{
		UISystem system = ModContent.GetInstance<UISystem>();
		player = Main.LocalPlayer;
		heart = StoragePlayer.LocalPlayer.GetStorageHeart();

		if (heart == null)
		{
			system.UI.SetState(null);
			return;
		}

		panel.Left.Pixels = opening ? -panel.Width.Pixels : panelLeft;

		system.inputs.Add(searchBar);
		system.inputs.Add(searchBar2);

		RefreshItems();
	}

	public override void OnDeactivate()
	{
		player = null;
		heart = null;

		items = null;

		easeTimer = 0;
		opening = false;
		closing = false;

		stackSplit = 0;
		stackDelay = 7;
		stackCounter = 0;

		sortMode = SortMode.Default;
		filterMode = FilterMode.All;

		sortButtons.choice = SortMode.index(sortMode);
		filterButtons.choice = FilterMode.index(filterMode);

		nameFilter = string.Empty;
		modFilter = string.Empty;

		UISystem system = ModContent.GetInstance<UISystem>();
		system.inputs.Remove(searchBar);
		system.inputs.Remove(searchBar2);
	}

	public void Open(bool animate = false)
	{
		easeTimer = 0;
		opening = animate;
		closing = false;

		UISystem system = ModContent.GetInstance<UISystem>();
		if (system.UI.CurrentState == this) return;

		system.UI.SetState(this);
	}

	public void Close(bool animate = false)
	{
		UISystem system = ModContent.GetInstance<UISystem>();
		if (system.UI.CurrentState == null) return;

		if (animate)
		{
			easeTimer = 0;
			opening = false;
			closing = true;
		}
		else
		{
			system.UI.SetState(null);
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (closing)
		{
			if (!AnimationClose())
			{
				Close();
			}

			return;
		}

		base.Update(gameTime);

		if (opening)
		{
			opening = AnimationOpen();
		}

		// TODO: Check access and heart changes

		isMouseHovering = false;
		foreach (UIElement e in Elements)
		{
			isMouseHovering |= e.IsMouseHovering;
		}

		player.mouseInterface |= isMouseHovering;

		UpdateStackTimer();
		UpdateStackSplit();
		UpdateCursor();

		if (nameFilter != searchBar.Text || modFilter != searchBar2.Text)
		{
			nameFilter = searchBar.Text;
			modFilter = searchBar2.Text;
			RefreshItems();
		}
	}

	public void UpdateCursor() {
		if (!GetItem(slotZone.mouseSlot).IsAir)
		{
			Main.cursorOverride = ItemSlot.ControlInUse ? 6 : Main.cursorOverride;
			Main.cursorOverride = ItemSlot.ShiftInUse   ? 8 : Main.cursorOverride;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (isMouseHovering)
		{
			filterButtons.DrawText(spriteBatch);
			sortButtons.DrawText(spriteBatch);
			slotZone.DrawText(spriteBatch);
		}
	}

	private void UpdateStackTimer()
	{
		if (stackSplit == 0)
		{
			stackCounter = 0;
			stackDelay = 7;
		}
		else
		{
			ReadOnlySpan<int> delays = stackalloc int[] { 5, 5, 5, 10, 15, 20, 25, 5 };

			if (++stackCounter >= delays[stackDelay])
			{
				if (--stackDelay < 2)
				{
					stackDelay = 2;
				}
				stackCounter = 0;
			}
		}

		if (!PlayerInput.Triggers.Current.MouseRight)
		{
			stackSplit = 0;
		}
		else if (stackSplit > 0)
		{
			stackSplit--;
		}
	}

	public void UpdateCounter()
	{
		IEnumerable<TEStorageUnit> units = heart.GetStorageUnits();
		int numItems = units.Sum(unit => unit.NumItems);

		IEnumerable<TEStorageUnit> activeUnits = units.Where(unit => unit.active);
		int capacity = activeUnits.Sum(unit => unit.Capacity);

		int len = capacityText.Text.Length;
		capacityText.SetText($"{numItems} / {capacity} Items");

		if (len != capacityText.Text.Length)
			capacityText.Recalculate();
	}

	private Item GetItem(int slot)
	{
		return slot >= 0 && slot < items.Count ? items[slot] : UISlotZone.Air;
	}

	public void RefreshItems()
	{
		items = ItemSorter.SortAndFilter(heart.GetStoredItems(), sortMode, filterMode, searchBar2.Text, searchBar.Text);

		UpdateCounter();

		slotZone.UpdateScrollBar((items.Count + columns - 1) / columns);
		slotZone.LoadItems();
	}

	private void ClickDeposit(UIMouseEvent e, UIElement depositButton)
	{
		if (TryDepositAll())
		{
			RefreshItems();
			SoundEngine.PlaySound(SoundID.Grab);
		}
	}

	private void PressSlotZone(UIEvent e, UIElement element)
	{
		int slot = slotZone.mouseSlot;
		bool changed = false;

		if (!Main.mouseItem.IsAir)
		{
			int oldStack = Main.mouseItem.stack;
			heart.Deposit(Main.mouseItem);
			changed = oldStack != Main.mouseItem.stack;
		}
		else if (slot >= 0 && slot < items.Count && !items[slot].IsAir)
		{
			Item item = items[slot].Clone();

			if (item.stack > item.maxStack)
			{
				item.stack = item.maxStack;
			}

			Main.mouseItem = heart.Withdraw(item);

			if (ItemSlot.ShiftInUse)
			{
				Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, GetItemSettings.InventoryUIToInventorySettings);
			}

			if (ItemSlot.ControlInUse)
			{
				player.trashItem = Main.mouseItem;
				Main.mouseItem = new Item();
			}

			changed = true;
		}

		if (changed)
		{
			RefreshItems();
			SoundEngine.PlaySound(SoundID.Grab);
		}
	}

	private void UpdateStackSplit()
	{
		if (!slotZone.IsMouseHovering || !Main.mouseRight)
		{
			return;
		}

		int slot = slotZone.mouseSlot;

		if (!GetItem(slot).IsAir && stackSplit <= 1)
		{
			stackSplit = stackSplit == 0 ? 15 : stackDelay;

			Item item = items[slot].Clone();
			item.stack = 1;

			bool changed = false;

			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = heart.Withdraw(item);
				changed = !Main.mouseItem.IsAir;
			}
			else if (Main.mouseItem.type == item.type && Main.mouseItem.stack < item.maxStack)
			{
				if (!heart.Withdraw(item).IsAir)
				{
					Main.mouseItem.stack += 1;
					changed = true;
				}
			}

			if (changed)
			{
				RefreshItems();
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}

	private bool TryDepositAll()
	{
		bool changed = false;
		for (int k = 10; k < 50; k++)
		{
			Item item = player.inventory[k];
			if (!item.favorited)
			{
				int oldStack = item.stack;
				heart.Deposit(item);
				changed |= oldStack != item.stack;
			}
		}
		return changed;
	}
}
