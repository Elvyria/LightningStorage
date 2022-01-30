using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

using ReLogic.Graphics;

using MagicStorage.UI;
using MagicStorage.Components;
using MagicStorage.Sorting;

namespace MagicStorage
{
	class StorageGUI : UIState
	{
		private bool isMouseHovering;

		private const float inventoryScale = 0.85f;

		private const int padding = 4;
		private const int columns = 10;

		private int stackSplit;
		private int stackDelay = 7;
		private int stackCounter = 0;

		private TEStorageHeart heart;
		private List<Item> items = new List<Item>();

		private UISlotZone slotZone;
		private UIText capacityText;
		private UISearchBar searchBar;
		private UISearchBar searchBar2;
		private UIButtonChoice sortButtons;
		private UIButtonChoice filterButtons;

		private SortMode sortMode = SortMode.Default;
		private FilterMode filterMode = FilterMode.All;
		private String nameFilter = string.Empty;
		private String modFilter = string.Empty;

		private Color lightBlue = new Color(73, 94, 171);
		private Color blue = new Color(63, 82, 151) * 0.7f;

		public override void OnInitialize()
		{
			float slotWidth = Main.inventoryBackTexture.Width * inventoryScale;
			float slotHeight = Main.inventoryBackTexture.Height * inventoryScale;

			float panelTop = Main.instance.invBottom + 60;
			float panelLeft = 20f;

			UIPanel panel = new UIPanel();
			float innerPanelLeft = panelLeft + panel.PaddingLeft;
			float innerPanelWidth = columns * (slotWidth + padding) + 20f + padding;
			float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
			float panelHeight = Main.screenHeight - panelTop - 40f;
			panel.Left.Set(panelLeft, 0f);
			panel.Top.Set(panelTop, 0f);
			panel.Width.Set(panelWidth, 0f);
			panel.Height.Set(panelHeight, 0f);
			panel.Recalculate();

			UIElement topBar = new UIElement();
			topBar.Width.Set(0f, 1f);
			topBar.Height.Set(32f, 0f);
			panel.Append(topBar);

			sortButtons = new UIButtonChoice(new Texture2D[]
					{
					Main.inventorySortTexture[0],
					MagicStorage.Instance.GetTexture("Assets/SortID"),
					MagicStorage.Instance.GetTexture("Assets/SortName"),
					MagicStorage.Instance.GetTexture("Assets/SortNumber")
					},
					new LocalizedText[]
					{
					Language.GetText("Mods.MagicStorage.Common.SortDefault"),
					Language.GetText("Mods.MagicStorage.Common.SortID"),
					Language.GetText("Mods.MagicStorage.Common.SortName"),
					Language.GetText("Mods.MagicStorage.Common.SortStack")
					});

			sortButtons.OnClick += (a, b) => {
				if ((SortMode)sortButtons.choice != sortMode)
				{
					sortMode = (SortMode)sortButtons.choice;
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

			filterButtons = new UIButtonChoice(new Texture2D[]
					{
					MagicStorage.Instance.GetTexture("Assets/FilterAll"),
					MagicStorage.Instance.GetTexture("Assets/FilterMelee"),
					MagicStorage.Instance.GetTexture("Assets/FilterPickaxe"),
					MagicStorage.Instance.GetTexture("Assets/FilterArmor"),
					MagicStorage.Instance.GetTexture("Assets/FilterPotion"),
					MagicStorage.Instance.GetTexture("Assets/FilterTile"),
					MagicStorage.Instance.GetTexture("Assets/FilterMisc"),
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

			filterButtons.OnClick += (a, b) => {
				if ((FilterMode)filterButtons.choice != filterMode)
				{
					filterMode = (FilterMode)filterButtons.choice;
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

			UIScrollbar scrollbar = new UIScrollbar();
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
			heart = null;
			items = null;

			sortMode = SortMode.Default;
			filterMode = FilterMode.All;
			sortButtons.choice = 0;
			filterButtons.choice = 0;
			nameFilter = string.Empty;
			modFilter = string.Empty;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// TODO: Check access and heart changes

			Main.HidePlayerCraftingMenu = true;
			foreach (UIElement element in Elements)
			{
				if (element.IsMouseHovering) {
					isMouseHovering = true;
					Main.LocalPlayer.mouseInterface = true;

					break;
				}
			}

			UpdateStackTimer();
			UpdateStackSplit();

			if (nameFilter != searchBar.Text || modFilter != searchBar2.Text)
			{
				nameFilter = searchBar.Text;
				modFilter = searchBar2.Text;
				RefreshItems();
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
				stackCounter++;
				int num = (stackDelay == 6) ? 25 : ((stackDelay == 5) ? 20 : ((stackDelay == 4) ? 15 : ((stackDelay != 3) ? 5 : 10)));
				if (stackCounter >= num)
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

			if (stackSplit > 0)
			{
				stackSplit--;
			}
		}

		private void UpdateCounter() {
			int numItems = 0;
			int capacity = 0;

			foreach (TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits())
			{
				if (abstractStorageUnit is TEStorageUnit storageUnit)
				{
					numItems += storageUnit.NumItems;
					capacity += storageUnit.Capacity;
				}
			}

			int len = capacityText.Text.Length;
			capacityText.SetText(numItems + "/" + capacity + " Items");

			if (len != capacityText.Text.Length)
				capacityText.Recalculate();
		}

		private Item GetItem(int slot)
		{
			Item item = slot >= 0 && slot < items.Count ? items[slot] : UISlotZone.Air;

			return item;
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
				Main.PlaySound(7, -1, -1, 1);
			}
		}

		private void PressSlotZone(UIEvent e, UIElement element)
		{
			int slot = slotZone.MouseSlot();
			bool changed = false;

			if (!Main.mouseItem.IsAir)
			{
				changed = TryDeposit(Main.mouseItem);
			}
			else if (slot >= 0 && slot < items.Count && !items[slot].IsAir)
			{
				Item item = items[slot].Clone();

				if (item.stack > item.maxStack)
				{
					item.stack = item.maxStack;
				}

				Main.mouseItem = DoWithdraw(item, ItemSlot.ShiftInUse);

				if (ItemSlot.ShiftInUse)
				{
					Main.mouseItem = Main.LocalPlayer.GetItem(Main.myPlayer, Main.mouseItem, false, true);
				}

				changed = true;
			}

			if (changed)
			{
				RefreshItems();
				Main.PlaySound(7, -1, -1, 1);
			}
		}

		private void UpdateStackSplit()
		{
			if (!slotZone.IsMouseHovering || !Main.mouseRight)
			{
				return;
			}

			int slot = slotZone.MouseSlot();
			bool changed = false;

			if (slot >= 0 && slot < items.Count && !items[slot].IsAir && stackSplit <= 1)
			{
				if (stackSplit == 0)
				{
					stackSplit = 15;
				}
				else
				{
					stackSplit = stackDelay;
				}

				Item item = items[slot].Clone();
				item.stack = 1;

				if (Main.mouseItem.IsAir)
				{
					Main.mouseItem = DoWithdraw(item);
					changed = true;
				}
				else if (Main.mouseItem.IsTheSameAs(item) && Main.mouseItem.stack < item.maxStack)
				{
					DoWithdraw(item);
					Main.mouseItem.stack += 1;
					changed = true;
				}
			}

			if (changed)
			{
				RefreshItems();
				Main.PlaySound(12, -1, -1, 1);
			}
		}

		private bool TryDeposit(Item item)
		{
			int oldStack = item.stack;
			DoDeposit(item);
			return oldStack != item.stack;
		}

		private void DoDeposit(Item item)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				heart.DepositItem(item);
			}
			else
			{
				NetHelper.SendDeposit(heart.ID, item);
				item.SetDefaults(0, true);
			}
		}

		private bool TryDepositAll()
		{
			Player player = Main.player[Main.myPlayer];
			bool changed = false;
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				for (int k = 10; k < 50; k++)
				{
					if (!player.inventory[k].IsAir && !player.inventory[k].favorited)
					{
						int oldStack = player.inventory[k].stack;
						heart.DepositItem(player.inventory[k]);
						if (oldStack != player.inventory[k].stack)
						{
							changed = true;
						}
					}
				}
			}
			else
			{
				List<Item> items = new List<Item>(40);
				for (int k = 10; k < 50; k++)
				{
					if (!player.inventory[k].IsAir && !player.inventory[k].favorited)
					{
						items.Add(player.inventory[k]);
					}
				}
				NetHelper.SendDepositAll(heart.ID, items);
				foreach (Item item in items)
				{
					item.SetDefaults(0, true);
				}
				changed = true;
			}
			return changed;
		}

		private Item DoWithdraw(Item item, bool toInventory = false)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				return heart.TryWithdraw(item);
			}

			NetHelper.SendWithdraw(heart.ID, item, toInventory);
			return new Item();
		}
	}
}
