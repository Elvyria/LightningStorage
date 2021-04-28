using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

using MagicStorage.Components;
using MagicStorage.Sorting;

namespace MagicStorage
{
	class CraftingGUI : UIState
	{
		private const float inventoryScale = 0.85f;
		private const float smallScale = 0.7f;

		private const int padding = 4;
		private const int columns = 10;
		private const int rows = 11;
		private const int columns2 = 7;

		private int stackSplit;
		private int stackDelay = 7;
		private int stackCounter = 0;

		private UITextPanel<LocalizedText> craftButton;
		private UIButtonChoice sortButtons;
		private UIButtonChoice filterButtons;
		private UIButtonChoice recipeButtons;

		private UISearchBar searchBar;
		private UISearchBar searchBar2;

		private UISlotZone stationZone;
		private UISlotZone recipeZone;
		private UISlotZone resultZone;

		private List<Item> items = new List<Item>();
		private Dictionary<int, int> itemCounts = new Dictionary<int, int>();
		private Dictionary<int, int> recipeGroupCounts = new Dictionary<int, int>(RecipeGroup.recipeGroups.Count);
		private Item resultItem = UISlotZone.Air;

		private TEStorageHeart heart;
		private TECraftingAccess access;

		private bool[] adjTiles = new bool[0];
		private bool adjWater = false;
		private bool adjLava = false;
		private bool adjHoney = false;
		private bool zoneSnow = false;
		private bool alchemyTable = false;

		private List<Recipe> recipes = new List<Recipe>();
		private List<Recipe> availableRecipes = new List<Recipe>();
		private List<bool> recipeAvailable = new List<bool>();

		private Recipe selectedRecipe;

		private UISlotZone previewZone;
		private UISlotZone ingredientZone;
		private UISlotZone storageZone;
		private UIText reqObjText2;

		private List<Item> storageItems = new List<Item>();
		private HashSet<ItemData> blockStorageItems = new HashSet<ItemData>();

		private SortMode sortMode = SortMode.Default;
		private FilterMode filterMode = FilterMode.All;
		private String nameFilter = string.Empty;
		private String modFilter = string.Empty;

		private static readonly Color lightBlue = new Color(73, 94, 171);
		private static readonly Color blue = new Color(63, 82, 151) * 0.7f;
		private static readonly Color darkBlue = new Color(30, 40, 100) * 0.7f;
		private static readonly Color deepBlue = new Color(163, 104, 176);

		public override void OnInitialize()
		{
			float itemSlotWidth = Main.inventoryBackTexture.Width * inventoryScale;
			float itemSlotHeight = Main.inventoryBackTexture.Height * inventoryScale;
			float smallSlotWidth = Main.inventoryBackTexture.Width * smallScale;
			float smallSlotHeight = Main.inventoryBackTexture.Height * smallScale;

			float panelTop = Main.instance.invBottom + 60;
			float panelLeft = 20f;

			UIPanel panel = new UIPanel();
			float innerPanelWidth = columns * (itemSlotWidth + padding) + 20f + padding;
			float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
			panel.Top.Set(Main.instance.invBottom + 60, 0f);
			panel.Left.Set(20f, 0f);
			panel.Width.Set(panelWidth, 0f);
			panel.Height.Set(Main.screenHeight - Main.instance.invBottom - 80f, 0f);

			UIPanel recipePanel = new UIPanel();
			float recipeLeft = panelLeft + panelWidth;
			float recipeWidth = columns2 * (smallSlotWidth + padding);
			recipeWidth += recipePanel.PaddingLeft + recipePanel.PaddingRight;
			recipePanel.Top = panel.Top;
			recipePanel.Left.Set(recipeLeft, 0f);
			recipePanel.Width.Set(recipeWidth, 0f);
			recipePanel.Height.Set(Main.screenHeight - Main.instance.invBottom - 350f, 0f);

			UIElement topBar = new UIElement();
			topBar.Width.Set(0f, 1f);
			topBar.Height.Set(32f, 0f);
			panel.Append(topBar);

			sortButtons = new UIButtonChoice(new Texture2D[]
					{
					Main.inventorySortTexture[0],
					MagicStorage.Instance.GetTexture("Assets/SortID"),
					MagicStorage.Instance.GetTexture("Assets/SortName")
					},
					new LocalizedText[]
					{
					Language.GetText("Mods.MagicStorage.SortDefault"),
					Language.GetText("Mods.MagicStorage.SortID"),
					Language.GetText("Mods.MagicStorage.SortName")
					});

			sortButtons.OnClick += ClickSortButtons;

			topBar.Append(sortButtons);

			float sortButtonsRight = sortButtons.GetDimensions().Width + padding;

			recipeButtons = new UIButtonChoice(new Texture2D[]
					{
					MagicStorage.Instance.GetTexture("Assets/RecipeAvailable"),
					MagicStorage.Instance.GetTexture("Assets/RecipeAll")
					},
					new LocalizedText[]
					{
					Language.GetText("Mods.MagicStorage.RecipeAvailable"),
					Language.GetText("Mods.MagicStorage.RecipeAll")
					});

			recipeButtons.OnClick += ClickRecipeButtons;
			float recipeButtonsLeft = sortButtonsRight + 32f + 3 * padding;
			recipeButtons.Left.Set(recipeButtonsLeft, 0f);
			topBar.Append(recipeButtons);
			float recipeButtonsRight = recipeButtonsLeft + recipeButtons.GetDimensions().Width + padding;

			searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.SearchName"));
			searchBar.Left.Set(recipeButtonsRight + padding, 0f);
			searchBar.Width.Set(-recipeButtonsRight - 2 * padding, 1f);
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
					Language.GetText("Mods.MagicStorage.FilterAll"),
					Language.GetText("Mods.MagicStorage.FilterWeapons"),
					Language.GetText("Mods.MagicStorage.FilterTools"),
					Language.GetText("Mods.MagicStorage.FilterEquips"),
					Language.GetText("Mods.MagicStorage.FilterPotions"),
					Language.GetText("Mods.MagicStorage.FilterTiles"),
					Language.GetText("Mods.MagicStorage.FilterMisc")
					});

			filterButtons.OnClick += ClickFilterButtons;

			topBar2.Append(filterButtons);

			float filterButtonsRight = filterButtons.GetDimensions().Width + padding;

			searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.SearchMod"));
			searchBar2.Left.Set(filterButtonsRight + padding, 0f);
			searchBar2.Width.Set(-filterButtonsRight - 2 * padding, 1f);
			searchBar2.Height.Set(0f, 1f);
			topBar2.Append(searchBar2);

			UIText stationText = new UIText(Language.GetText("Mods.MagicStorage.CraftingStations"));
			stationText.Top.Set(76f, 0f);
			panel.Append(stationText);

			stationZone = new UISlotZone(GetStation, inventoryScale);
			stationZone.Top.Set(100f, 0f);
			stationZone.SetDimensions(columns, 1);
			stationZone.OnMouseDown += PressStation;
			panel.Append(stationZone);

			UIText recipeText = new UIText(Language.GetText("Mods.MagicStorage.Recipes"));
			recipeText.Top.Set(152f, 0f);
			panel.Append(recipeText);

			recipeZone = new UISlotZone(GetRecipe, GetRecipeColor, inventoryScale);
			recipeZone.Top.Set(176f, 0f);
			recipeZone.OnMouseDown += PressRecipe;
			panel.Append(recipeZone);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.Left.Set(10, 0);
			recipeZone.SetScrollbar(scrollbar);
			panel.Append(scrollbar);

			recipeZone.SetDimensions(columns, rows);

			UIText recipePanelHeader = new UIText(Language.GetText("Mods.MagicStorage.SelectedRecipe"));
			recipePanel.Append(recipePanelHeader);

			UIText ingredientText = new UIText(Language.GetText("Mods.MagicStorage.Ingredients"));
			ingredientText.Top.Set(30f, 0f);
			recipePanel.Append(ingredientText);

			previewZone = new UISlotZone(GetSelectedItem, inventoryScale);
			previewZone.Top.Set(0f, 0f);
			previewZone.Left.Set(-itemSlotWidth, 1f);
			recipePanel.Append(previewZone);

			ingredientZone = new UISlotZone(GetIngredient, smallScale);
			ingredientZone.Top.Set(54f, 0f);
			ingredientZone.SetDimensions(columns2, 2);
			ingredientZone.OnMouseDown += PressIngredient;
			recipePanel.Append(ingredientZone);

			UIText reqObjText = new UIText(Language.GetText("LegacyInterface.22"));
			reqObjText.Top.Set(136f, 0f);
			recipePanel.Append(reqObjText);

			reqObjText2 = new UIText(String.Empty);
			reqObjText2.Top.Set(160f, 0f);
			recipePanel.Append(reqObjText2);

			UIText storedItemsText = new UIText(Language.GetText("Mods.MagicStorage.StoredItems"));
			storedItemsText.Top.Set(190f, 0f);
			recipePanel.Append(storedItemsText);

			storageZone = new UISlotZone(GetStorage, smallScale);
			storageZone.Top.Set(214f, 0f);
			storageZone.SetDimensions(7, 4);
			recipePanel.Append(storageZone);

			craftButton = new UITextPanel<LocalizedText>(Language.GetText("LegacyMisc.72"), 1f);
			craftButton.Top.Set(-32f, 1f);
			craftButton.Width.Set(100f, 0f);
			craftButton.Height.Set(24f, 0f);
			craftButton.PaddingTop = 8f;
			craftButton.PaddingBottom = 8f;

			recipePanel.Append(craftButton);

			resultZone = new UISlotZone(GetResultItem, inventoryScale);
			resultZone.Left.Set(-itemSlotWidth, 1f);
			resultZone.Top.Set(-itemSlotHeight, 1f);
			resultZone.OnMouseDown += PressResult;
			recipePanel.Append(resultZone);

			Append(panel);
			Append(recipePanel);
		}

		public override void OnActivate()
		{
			heart = StoragePlayer.LocalPlayer.GetStorageHeart();
			access = StoragePlayer.LocalPlayer.GetCraftingAccess();

			if (access == null || heart == null)
			{
				Deactivate();
				return;
			}

			RefreshItems();
		}

		public override void OnDeactivate()
		{
			Main.HidePlayerCraftingMenu = false;

			heart = null;
			access = null;
			recipes = null;
			recipeAvailable = null;
			selectedRecipe = null;

			filterButtons.choice = 0;
			sortButtons.choice = 0;
			recipeButtons.choice = 0;

			filterMode = FilterMode.All;
			sortMode = SortMode.Default;
			modFilter = string.Empty;
			nameFilter = string.Empty;
		}

		private byte scale(int n, int max)
		{
			return (byte)((n * 255) / max);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// TODO: Check access and heart changes

			Main.HidePlayerCraftingMenu = true;

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			UpdateStackTimer();
			UpdateCraftButton();

			if (nameFilter != searchBar.Text || modFilter != searchBar2.Text)
			{
				nameFilter = searchBar.Text;
				modFilter = searchBar2.Text;
				RefreshItems();
			}
		}

		private void UpdateCraftButton()
		{
			if (selectedRecipe == null || !IsAvailable(selectedRecipe))
			{
				craftButton.BackgroundColor = darkBlue;
				return;
			}

			if (craftButton.IsMouseHovering)
			{
				craftButton.BackgroundColor = lightBlue;

				if (Main.mouseLeft && stackSplit <= 1)
				{
					if (stackSplit == 0)
					{
						stackSplit = 15;
					}
					else
					{
						stackSplit = stackDelay;
					}

					TryCraft();
					RefreshItems();
					Main.PlaySound(7, -1, -1, 1);
				}
			}
			else
			{
				craftButton.BackgroundColor = blue;
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

			if (!PlayerInput.Triggers.Current.MouseLeft)
			{
				stackSplit = 0;
			}

			if (stackSplit > 0)
			{
				stackSplit--;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (IsMouseHovering)
			{
				recipeButtons.DrawText(spriteBatch);
				filterButtons.DrawText(spriteBatch);
				sortButtons.DrawText(spriteBatch);
				stationZone.DrawText(spriteBatch);
				storageZone.DrawText(spriteBatch);
				recipeZone.DrawText(spriteBatch);
				ingredientZone.DrawText(spriteBatch);
				resultZone.DrawText(spriteBatch);
				previewZone.DrawText(spriteBatch);
			}
		}

		private Item GetStation(int slot)
		{
			if (slot < access.stations.Length)
			{
				return access.stations[slot];
			}

			return UISlotZone.Air;
		}

		private Item GetRecipe(int slot)
		{
			if (slot < recipes.Count)
			{
				return recipes[slot].createItem;
			}

			return UISlotZone.Air;
		}

		private Item GetIngredient(int slot)
		{
			if (selectedRecipe != null && slot < selectedRecipe.requiredItem.Length)
			{
				Item item = selectedRecipe.requiredItem[slot].Clone();

				RecipeGroup recipeGroup = RecipeGroup.recipeGroups[RecipeGroupID.Wood];
				if (selectedRecipe.anyWood && recipeGroup.ContainsItem(item.type))
				{
					item.SetNameOverride(recipeGroup.GetText());
					return item;
				}

				recipeGroup = RecipeGroup.recipeGroups[RecipeGroupID.Wood];
				if (selectedRecipe.anySand && recipeGroup.ContainsItem(item.type))
				{
					item.SetNameOverride(recipeGroup.GetText());
					return item;
				}

				recipeGroup = RecipeGroup.recipeGroups[RecipeGroupID.Wood];
				if (selectedRecipe.anyIronBar && recipeGroup.ContainsItem(item.type))
				{
					item.SetNameOverride(recipeGroup.GetText());
					return item;
				}

				recipeGroup = RecipeGroup.recipeGroups[RecipeGroupID.Wood];
				if (selectedRecipe.anyFragment && recipeGroup.ContainsItem(item.type))
				{
					item.SetNameOverride(recipeGroup.GetText());
					return item;
				}

				recipeGroup = RecipeGroup.recipeGroups[RecipeGroupID.PressurePlate];
				if (selectedRecipe.anyPressurePlate && recipeGroup.ContainsItem(item.type))
				{
					item.SetNameOverride(recipeGroup.GetText());
					return item;
				}

				if (selectedRecipe.ProcessGroupsForText(item.type, out string nameOverride))
				{
					item.SetNameOverride(nameOverride);
				}

				return item;
			}

			return UISlotZone.Air;
		}

		private Item GetStorage(int slot)
		{
			if (slot < storageItems.Count)
			{
				return storageItems[slot];
			}

			return UISlotZone.Air;
		}

		private Color GetRecipeColor(int slot)
		{
			if (slot < recipes.Count)
			{
				if (recipeButtons.choice == 0)
				{
					if (recipes[slot] == selectedRecipe)
						return Color.Lime;

					return Color.White;
				}
				else
				{
					if (recipes[slot] == selectedRecipe)
						return Color.Lime;

					if (recipeAvailable[slot])
						return Color.White;

					return deepBlue;
				}
			}

			return Color.White;
		}

		private Item GetSelectedItem(int slot)
		{
			if (selectedRecipe != null)
			{
				return selectedRecipe.createItem;
			}

			return UISlotZone.Air;
		}

		private Item GetResultItem(int _)
		{
			if (selectedRecipe != null && resultItem != null)
			{
				return resultItem;
			}

			return UISlotZone.Air;
		}

		private void UpdateRecipeText()
		{
			bool isEmpty = true;
			StringBuilder builder = new StringBuilder("");

			for (int k = 0; k < selectedRecipe.requiredTile.Length; k++)
			{
				if (selectedRecipe.requiredTile[k] == -1)
				{
					break;
				}

				if (!isEmpty) builder.Append(", ");

				builder.Append(Lang.GetMapObjectName(MapHelper.TileToLookup(selectedRecipe.requiredTile[k], 0)));
				isEmpty = false;
			}

			if (selectedRecipe.needWater)
			{
				if (!isEmpty) builder.Append(", ");

				builder.Append(Language.GetTextValue("LegacyInterface.53"));
				isEmpty = false;
			}

			if (selectedRecipe.needHoney)
			{
				if (!isEmpty) builder.Append(", ");

				builder.Append(Language.GetTextValue("LegacyInterface.58"));
				isEmpty = false;
			}

			if (selectedRecipe.needLava)
			{
				if (!isEmpty) builder.Append(", ");

				builder.Append(Language.GetTextValue("LegacyInterface.56"));
				isEmpty = false;
			}

			if (selectedRecipe.needSnowBiome)
			{
				if (!isEmpty) builder.Append(", ");

				builder.Append(Language.GetTextValue("LegacyInterface.123"));
				isEmpty = false;
			}

			if (isEmpty)
			{
				reqObjText2.SetText(Language.GetTextValue("LegacyInterface.23"));
			}
			else
			{
				reqObjText2.SetText(builder.ToString());
			}
		}

		public void RefreshItems()
		{
			items = ItemSorter.SortAndFilter(heart.GetStoredItems(), SortMode.Id, FilterMode.All, "", "");

			AnalyzeIngredients();

			RefreshStorageItems();
			RefreshRecipes();
		}

		private void RefreshRecipes()
		{
			Recipe[] temp = ItemSorter.SortAndFilter(Main.recipe, sortMode, filterMode, searchBar2.Text, searchBar.Text);

			itemCounts.Clear();

			foreach (int key in RecipeGroup.recipeGroups.Keys)
			{
				recipeGroupCounts[key] = 0;
			}

			foreach (Item item in items)
			{
				if (itemCounts.ContainsKey(item.netID))
				{
					itemCounts[item.netID] += item.stack;
				}
				else
				{
					itemCounts[item.netID] = item.stack;
				}

				foreach (KeyValuePair<int, RecipeGroup> pair in RecipeGroup.recipeGroups)
				{
					if (pair.Value.ContainsItem(item.type))
					{
						recipeGroupCounts[pair.Key] += item.stack;
					}
				}
			}

			if (recipeButtons.choice == 0)
			{
				recipes = temp.AsParallel().AsOrdered().Where(recipe => IsAvailable(recipe)).ToList();
				recipeAvailable = temp.Select(recipe => true).ToList();
			}
			else
			{
				recipes = temp.ToList();
				recipeAvailable = temp.AsParallel().AsOrdered().Select(recipe => IsAvailable(recipe)).ToList();
			}

			recipeZone.UpdateScrollBar((recipes.Count + columns - 1) / columns);
		}

		private void AnalyzeIngredients()
		{
			Player player = Main.player[Main.myPlayer];

			if (adjTiles.Length != player.adjTile.Length)
			{
				Array.Resize(ref adjTiles, player.adjTile.Length);
			}

			for (int k = 0; k < adjTiles.Length; k++)
			{
				adjTiles[k] = false;
			}

			adjWater = false;
			adjLava = false;
			adjHoney = false;
			zoneSnow = false;
			alchemyTable = false;

			foreach (Item item in access.stations)
			{
				if (item.createTile >= 0)
				{
					adjTiles[item.createTile] = true;
					if (item.createTile == TileID.GlassKiln || item.createTile == TileID.Hellforge || item.createTile == TileID.AdamantiteForge)
					{
						adjTiles[TileID.Furnaces] = true;
					}
					if (item.createTile == TileID.AdamantiteForge)
					{
						adjTiles[TileID.Hellforge] = true;
					}
					if (item.createTile == TileID.MythrilAnvil)
					{
						adjTiles[TileID.Anvils] = true;
					}
					if (item.createTile == TileID.BewitchingTable || item.createTile == TileID.Tables2)
					{
						adjTiles[TileID.Tables] = true;
					}
					if (item.createTile == TileID.AlchemyTable)
					{
						adjTiles[TileID.Bottles] = true;
						adjTiles[TileID.Tables] = true;
						alchemyTable = true;
					}
					bool[] oldAdjTile = player.adjTile;
					bool oldAdjWater = adjWater;
					bool oldAdjLava = adjLava;
					bool oldAdjHoney = adjHoney;
					bool oldAlchemyTable = alchemyTable;
					player.adjTile = adjTiles;
					player.adjWater = false;
					player.adjLava = false;
					player.adjHoney = false;
					player.alchemyTable = false;
					TileLoader.AdjTiles(player, item.createTile);
					if (player.adjWater)
					{
						adjWater = true;
					}
					if (player.adjLava)
					{
						adjLava = true;
					}
					if (player.adjHoney)
					{
						adjHoney = true;
					}
					if (player.alchemyTable)
					{
						alchemyTable = true;
					}
					player.adjTile = oldAdjTile;
					player.adjWater = oldAdjWater;
					player.adjLava = oldAdjLava;
					player.adjHoney = oldAdjHoney;
					player.alchemyTable = oldAlchemyTable;
				}
				if (item.type == ItemID.WaterBucket || item.type == ItemID.BottomlessBucket)
				{
					adjWater = true;
				}
				if (item.type == ItemID.LavaBucket)
				{
					adjLava = true;
				}
				if (item.type == ItemID.HoneyBucket)
				{
					adjHoney = true;
				}
				if (item.type == MagicStorage.Instance.ItemType("SnowBiomeEmulator"))
				{
					zoneSnow = true;
				}
			}
			adjTiles[MagicStorage.Instance.TileType("CraftingAccess")] = true;
		}

		private bool IsAvailable(Recipe recipe)
		{
			bool water = !recipe.needWater || adjWater || adjTiles[TileID.Sinks];
			bool honey = !recipe.needHoney || recipe.needHoney == adjHoney;
			bool lava = !recipe.needLava || recipe.needLava == adjLava;
			bool snow = !recipe.needSnowBiome || zoneSnow; // TODO: Add snow zone tile check

			if (!(water && honey && lava && snow))
			{
				return false;
			}

			foreach (int tile in recipe.requiredTile)
			{
				if (tile == -1)
				{
					break;
				}

				if (!adjTiles[tile])
				{
					return false;
				}
			}

			foreach (Item ingredient in recipe.requiredItem)
			{
				if (ingredient.type == 0)
				{
					break;
				}

				if (itemCounts.ContainsKey(ingredient.netID) && itemCounts[ingredient.netID] >= ingredient.stack
						|| recipe.anyWood && RecipeGroup.recipeGroups[RecipeGroupID.Wood].ContainsItem(ingredient.type) && recipeGroupCounts[RecipeGroupID.Wood] >= ingredient.stack
						|| recipe.anySand && RecipeGroup.recipeGroups[RecipeGroupID.Sand].ContainsItem(ingredient.type) && recipeGroupCounts[RecipeGroupID.Sand] >= ingredient.stack
						|| recipe.anyIronBar && RecipeGroup.recipeGroups[RecipeGroupID.IronBar].ContainsItem(ingredient.type) && recipeGroupCounts[RecipeGroupID.IronBar] >= ingredient.stack
						|| recipe.anyIronBar && RecipeGroup.recipeGroups[RecipeGroupID.Fragment].ContainsItem(ingredient.type) && recipeGroupCounts[RecipeGroupID.Fragment] >= ingredient.stack
						|| recipe.anyPressurePlate && RecipeGroup.recipeGroups[RecipeGroupID.PressurePlate].ContainsItem(ingredient.type) && recipeGroupCounts[RecipeGroupID.PressurePlate] >= ingredient.stack)
				{
					continue;
				}

				bool enough = false;

				foreach (int id in recipe.acceptedGroups)
				{
					if (RecipeGroup.recipeGroups[id].ContainsItem(ingredient.type) && recipeGroupCounts[id] >= ingredient.stack)
					{
						enough = true;
						break;
					}
				}

				if (!enough)
				{
					return false;
				}
			}

			return true;
		}

		private void RefreshStorageItems()
		{
			storageItems.Clear();
			resultItem = UISlotZone.Air;

			if (selectedRecipe != null)
			{
				foreach (Item item in items)
				{
					foreach (Item ingredient in selectedRecipe.requiredItem)
					{
						if (ingredient.type == 0)
						{
							break;
						}
						if (item.type == ingredient.type || RecipeGroupMatch(selectedRecipe, ingredient.type, item.type))
						{
							storageItems.Add(item);
						}
					}

					if (item.IsTheSameAs(selectedRecipe.createItem))
					{
						resultItem = item;
					}
				}
			}
		}

		private bool RecipeGroupMatch(Recipe recipe, int type1, int type2)
		{
			return recipe.useWood(type1, type2) || recipe.useSand(type1, type2) || recipe.useIronBar(type1, type2) || recipe.useFragment(type1, type2) || recipe.AcceptedByItemGroups(type1, type2) || recipe.usePressurePlate(type1, type2);
		}

		private void PressIngredient(UIEvent _event, UIElement _element)
		{
			if (selectedRecipe == null)
			{
				return;
			}

			int slot = ingredientZone.MouseSlot();

			if (slot >= 0 && slot < selectedRecipe.requiredItem.Length)
			{
				Item ingredient = selectedRecipe.requiredItem[slot];
				foreach (Recipe recipe in Main.recipe)
				{
					if (recipe.createItem.IsTheSameAs(ingredient))
					{
						SelectRecipe(recipe);
						return;
					}
				}
			}
		}

		private void ClickRecipeButtons(UIEvent _event, UIElement _element)
		{
			RefreshRecipes();
			Main.PlaySound(12, -1, -1, 1);
		}

		private void ClickFilterButtons(UIEvent _event, UIElement _element)
		{
			if ((FilterMode)filterButtons.choice != filterMode)
			{
				filterMode = (FilterMode)filterButtons.choice;
				RefreshRecipes();
				Main.PlaySound(12, -1, -1, 1);
			}
		}

		private void ClickSortButtons(UIEvent _event, UIElement _element)
		{
			if ((SortMode)sortButtons.choice != sortMode)
			{
				sortMode = (SortMode)sortButtons.choice;
				RefreshRecipes();
				Main.PlaySound(12, -1, -1, 1);
			}
		}

		private void PressRecipe(UIEvent _event, UIElement _element)
		{
			int slot = recipeZone.MouseSlot();

			if (slot >= 0 && slot < recipes.Count())
			{
				Recipe recipe = recipes[slot];
				if (!recipe.createItem.IsAir)
				{
					SelectRecipe(recipe);
					Main.PlaySound(12, -1, -1, 1);
				}
			}
		}

		private void PressStation(UIEvent _event, UIElement _element)
		{
			int slot = stationZone.MouseSlot();

			if (slot < 0 || slot >= access.stations.Length || (!ItemSlot.ShiftInUse && Main.mouseItem.IsAir && access.stations[slot].IsAir))
			{
				return;
			}

			Player player = Main.LocalPlayer;

			if (ItemSlot.ShiftInUse)
			{
				Item station = player.GetItem(Main.myPlayer, WithdrawStation(slot), false, true);
				if (!station.IsAir)
				{
					player.QuickSpawnClonedItem(station);
				}
			}
			else if (Main.mouseItem.IsTheSameAs(access.stations[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack)
			{
				WithdrawStation(slot);
				Main.mouseItem.stack += 1;
			}
			else
			{
				Main.mouseItem = SwapStations(Main.mouseItem, slot);
			}

			RefreshItems();
			Main.PlaySound(7, -1, -1, 1);
		}

		private void PressResult(UIEvent _event, UIElement _element)
		{
			int slot = resultZone.MouseSlot();
			if (slot == 0 && selectedRecipe != null)
			{
				if (!Main.mouseItem.IsAir)
				{
					TryDepositResult(Main.mouseItem);
				}
				else
				{
					Item withdraw = resultItem.Clone();
					if (withdraw.stack < withdraw.maxStack)
					{
						withdraw.stack = withdraw.maxStack;
					}

					Main.mouseItem = WithdrawItem(resultItem.Clone());

					if (ItemSlot.ShiftInUse)
					{
						Main.mouseItem = Main.LocalPlayer.GetItem(Main.myPlayer, withdraw, false, true);
					}
				}

				RefreshItems();
				Main.PlaySound(7, -1, -1, 1);
			}
		}

		private void SelectRecipe(Recipe recipe)
		{
			if (recipe != null && selectedRecipe != recipe)
			{
				selectedRecipe = recipe;

				UpdateRecipeText();

				RefreshStorageItems();
			}
		}

		private Item WithdrawStation(int slot)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				return access.TryWithdrawStation(slot);
			}

			NetHelper.SendWithdrawStation(access.ID, slot);
			return new Item();
		}

		private Item SwapStations(Item item, int slot)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				return access.DoStationSwap(item, slot);
			}

			NetHelper.SendStationSlotClick(access.ID, item, slot);
			return new Item();
		}

		private void TryCraft()
		{
			List<Item> availableItems = new List<Item>(storageItems.Where(item => !blockStorageItems.Contains(new ItemData(item))).Select(item => item.Clone()));
			List<Item> toWithdraw = new List<Item>(selectedRecipe.requiredItem.Length);
			foreach (Item item in selectedRecipe.requiredItem)
			{
				if (item.type == 0)
				{
					break;
				}

				int stack = item.stack;
				ModRecipe modRecipe = selectedRecipe as ModRecipe;
				if (modRecipe != null)
				{
					stack = modRecipe.ConsumeItem(item.type, item.stack);
				}

				if (selectedRecipe.alchemy && alchemyTable)
				{
					int save = 0;
					for (int j = 0; j < stack; j++)
					{
						if (Main.rand.Next(3) == 0)
						{
							save++;
						}
					}
					stack -= save;
				}

				if (stack <= 0)
				{
					continue;
				}

				foreach (Item tryItem in availableItems)
				{
					if (item.type == tryItem.type || RecipeGroupMatch(selectedRecipe, item.type, tryItem.type))
					{
						if (tryItem.stack > stack)
						{
							Item temp = tryItem.Clone();
							temp.stack = stack;
							toWithdraw.Add(temp);
							tryItem.stack -= stack;
							stack = 0;
						}
						else
						{
							toWithdraw.Add(tryItem.Clone());
							stack -= tryItem.stack;
							tryItem.stack = 0;
							tryItem.type = 0;
						}
					}
				}
			}

			Item resultItem = selectedRecipe.createItem.Clone();
			resultItem.Prefix(-1);

			RecipeHooks.OnCraft(resultItem, selectedRecipe);
			ItemLoader.OnCraft(resultItem, selectedRecipe);

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				foreach (Item item in DoCraft(heart, toWithdraw, resultItem))
				{
					Main.player[Main.myPlayer].QuickSpawnClonedItem(item, item.stack);
				}
			}
			else if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetHelper.SendCraftRequest(heart.ID, toWithdraw, resultItem);
			}
		}

		internal List<Item> DoCraft(TEStorageHeart heart, List<Item> toWithdraw, Item result)
		{
			List<Item> items = new List<Item>();
			foreach (Item tryWithdraw in toWithdraw)
			{
				Item withdrawn = heart.TryWithdraw(tryWithdraw);
				if (!withdrawn.IsAir)
				{
					items.Add(withdrawn);
				}

				if (withdrawn.stack < tryWithdraw.stack)
				{
					for (int k = 0; k < items.Count; k++)
					{
						heart.DepositItem(items[k]);
						if (items[k].IsAir)
						{
							items.RemoveAt(k);
							k--;
						}
					}
					return items;
				}
			}

			items.Clear();
			heart.DepositItem(result);
			if (!result.IsAir)
			{
				items.Add(result);
			}

			return items;
		}

		private bool TryDepositResult(Item item)
		{
			int oldStack = item.stack;
			DepositItem(item);
			return oldStack != item.stack;
		}

		private void DepositItem(Item item)
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

		private Item WithdrawItem(Item item, bool toInventory = false)
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
