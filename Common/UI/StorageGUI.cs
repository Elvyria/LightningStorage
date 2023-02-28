using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.GameInput;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

using MagicStorage.Common.Players;
using MagicStorage.Common.Sorting;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Common.UI;

class StorageGUI : UIState
{
	private bool isMouseHovering;

	private const float inventoryScale = 0.8f;

	private const int padding = 4;
	private const int columns = 10;

	private int stackSplit;
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

	private static readonly Color lightBlue = new Color(73, 94, 171);
	private static readonly Color blue = new Color(63, 82, 151) * 0.7f;

	public override void OnInitialize()
	{
		float slotWidth = TextureAssets.InventoryBack9.Width() * inventoryScale;
		float slotHeight = TextureAssets.InventoryBack9.Height() * inventoryScale;

		float panelTop = Main.instance.invBottom + 60;
		const float panelLeft = 20f;

		panel = new UIPanel();

		float innerPanelLeft = panelLeft + panel.PaddingLeft;
		float innerPanelWidth = columns * (slotWidth + padding) + 20f + padding;
		float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
		float panelHeight = Main.screenHeight - panelTop - 20f;

		panel.Left.Set(panelLeft, 0f);
		panel.Top.Set(panelTop, 0f);
		panel.Width.Set(panelWidth, 0f);
		panel.Height.Set(panelHeight, 0f);
		panel.Recalculate();

		UIElement topBar = new UIElement();
		topBar.Width.Set(0f, 1f);
		topBar.Height.Set(32f, 0f);
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

		sortButtons.OnClick += (a, b) =>
		{
			IComparer<Item> newSort = SortMode.from(sortButtons.choice);
			if (newSort != sortMode)
			{
				sortMode = newSort;
				RefreshItems();
			}
		};

		topBar.Append(sortButtons);

		UITextPanel<LocalizedText> depositButton = new UITextPanel<LocalizedText>(Language.GetText("Mods.MagicStorage.Common.DepositAll"), 1f);
		depositButton.Left.Set(sortButtons.GetDimensions().Width + 2 * padding, 0f);
		depositButton.Width.Set(128f, 0f);
		depositButton.Height.Set(-2 * padding, 1f);
		depositButton.PaddingTop = 8f;
		depositButton.PaddingBottom = 8f;
		depositButton.OnClick += ClickDeposit;
		depositButton.OnMouseOver += (a, b) => depositButton.BackgroundColor = lightBlue;
		depositButton.OnMouseOut += (a, b) => depositButton.BackgroundColor = blue;

		topBar.Append(depositButton);

		float depositButtonRight = sortButtons.GetDimensions().Width + 2 * padding + depositButton.GetDimensions().Width;
		searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchName"));
		searchBar.Left.Set(depositButtonRight + padding, 0f);
		searchBar.Width.Set(-depositButtonRight - 2 * padding, 1f);
		searchBar.Height.Set(0f, 1f);

		topBar.Append(searchBar);

		UIElement topBar2 = new UIElement();
		topBar2.Width.Set(0f, 1f);
		topBar2.Height.Set(32f, 0f);
		topBar2.Top.Set(36f, 0f);

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

		filterButtons.OnClick += (a, b) =>
		{
			IFilter<Item> newFilter = FilterMode.from(filterButtons.choice);
			if (newFilter != filterMode)
			{
				filterMode = newFilter;
				RefreshItems();
			}
		};

		topBar2.Append(filterButtons);

		searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchMod"));
		searchBar2.Left.Set(depositButtonRight + padding, 0f);
		searchBar2.Width.Set(-depositButtonRight - 2 * padding, 1f);
		searchBar2.Height.Set(0f, 1f);

		topBar2.Append(searchBar2);

		slotZone = new UISlotZone(GetItem, inventoryScale);
		slotZone.Width.Set(0f, 1f);
		slotZone.Top.Set(76f, 0f);
		slotZone.Height.Set(-116f, 1f);
		slotZone.OnMouseDown += PressSlotZone;

		panel.Append(slotZone);

		UIScrollableBar scrollbar = new UIScrollableBar();
		scrollbar.Left.Set(10, 0);
		slotZone.SetScrollbar(scrollbar);

		panel.Append(scrollbar);

		capacityText = new UIText(string.Empty);
		capacityText.Top.Set(-32f, 1f);
		capacityText.Left.Set(6f, 0f);
		capacityText.Height.Set(32f, 0);
		panel.Append(capacityText);

		float slotZoneHeight = capacityText.GetDimensions().Y - (filterButtons.GetDimensions().Y + filterButtons.GetDimensions().Height);
		slotZone.SetDimensions(columns, (int)(slotZoneHeight / (slotHeight + slotZone.padding)));

		Append(panel);
	}

	public override void OnActivate()
	{
		player = Main.LocalPlayer;
		heart = StoragePlayer.LocalPlayer.GetStorageHeart();

		if (heart == null)
		{
			Deactivate();
			return;
		}

		RefreshItems();
	}

	public override void OnDeactivate()
	{
		player = null;
		heart = null;

		items = null;

		sortMode = SortMode.Default;
		filterMode = FilterMode.All;

		sortButtons.choice = SortMode.Default.index();
		filterButtons.choice = FilterMode.All.index();

		nameFilter = string.Empty;
		modFilter = string.Empty;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

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
			if (ItemSlot.ControlInUse)
			{
				Main.cursorOverride = 6;
			}

			if (ItemSlot.ShiftInUse)
			{
				Main.cursorOverride = 8;
			}
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
			if (!item.IsAir && item.stack > 0 && !item.favorited)
			{
				int oldStack = item.stack;
				heart.Deposit(item);
				changed |= oldStack != item.stack;
			}
		}
		return changed;
	}
}
