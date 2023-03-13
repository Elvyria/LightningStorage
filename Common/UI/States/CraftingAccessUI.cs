using System.Numerics;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.Map;

using MagicStorage.Common.Players;
using MagicStorage.Common.Sorting;
using MagicStorage.Common.Systems;
using MagicStorage.Content.TileEntities;

namespace MagicStorage.Common.UI.States;

class CraftingAccessUI : UIState
{
#nullable disable
	private bool isMouseHovering;

	private const float inventoryScale = 0.8f;
	private const float smallScale = 0.7f;

	private const float padding = 4f;

	const float panelLeft = 20f;

	private const int columns = 10;
	private const int columns2 = 7;

	private const int recipePanelColumns = 7;
	private const int ingredientRows = 2;
	private const int storageRows = 4;

	private float easeTimer = 0;
	public bool opening = false;
	public bool closing = false;

	private int stackSplit = 0;
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

	private List<Item> items;
	private List<Item> ingredients;
	private List<byte> craftableIngredients;

	private Dictionary<int, int> itemCounts;
	private Dictionary<int, int> recipeGroupCounts;

	private Item resultItem = UISlotZone.Air;

	private Player player;
	private TEStorageHeart heart;
	private TECraftingAccess access;

	private bool[] adjTiles;
	private bool[] adjLiquids;
	private List<Recipe.Condition> conditions;

	private Recipe[] vanillaRecipes;
	private List<Recipe> recipes;

	private bool[] recipeAvailable;

	private Recipe selectedRecipe;

	private UIPanel panel;
	private UIPanel recipePanel;

	private UISlotZone previewZone;
	private UISlotZone ingredientZone;
	private UISlotZone storageZone;

	private UIText reqObjText2;

	private List<Item> storageItems;

	private RecipeMode recipeMode = RecipeMode.Available;
	private IComparer<Item> sortMode = SortMode.Default;
	private IFilter<Item> filterMode = FilterMode.All;
	private string nameFilter = string.Empty;
	private string modFilter = string.Empty;
#nullable restore

	public override void OnInitialize()
	{
		float slotWidth = TextureAssets.InventoryBack9.Width() * inventoryScale;
		float slotHeight = TextureAssets.InventoryBack9.Height() * inventoryScale;
		float smallSlotWidth = TextureAssets.InventoryBack9.Width() * smallScale;
		float smallSlotHeight = TextureAssets.InventoryBack9.Height() * smallScale;

		float panelTop = Main.instance.invBottom + 60f;
		const float pannelBottom = 20f;

		panel = new UIPanel();

		float innerPanelLeft = panelLeft + panel.PaddingLeft;
		float innerPanelWidth = columns * (slotWidth + padding) + 20f + padding;
		float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
		float panelHeight = Main.screenHeight - panelTop - pannelBottom;

		panel.Left.Set((int)(opening ? -panelWidth : panelLeft), 0f);
		panel.Top.Pixels = panelTop;
		panel.Width.Pixels = panelWidth;
		panel.Height.Pixels = panelHeight;
		panel.Recalculate();

		recipePanel = new UIPanel();
		float recipeLeft = panelLeft + panelWidth;
		float recipeWidth = columns2 * (smallSlotWidth + padding);
		recipeWidth += recipePanel.PaddingLeft + recipePanel.PaddingRight;
		recipePanel.Top = panel.Top;
		recipePanel.Left.Pixels = recipeLeft;
		recipePanel.Width.Pixels = recipeWidth;

		UIElement topBar = new UIElement();
		topBar.Width.Percent = 1f;
		topBar.Height.Pixels = 32f;
		panel.Append(topBar);

		sortButtons = new UIButtonChoice(new Asset<Texture2D>[]
				{
					TextureAssets.InventorySort[0],
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortID"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortName")
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.Common.SortDefault"),
					Language.GetText("Mods.MagicStorage.Common.SortID"),
					Language.GetText("Mods.MagicStorage.Common.SortName")
				});

		sortButtons.OnClick += ClickSortButtons;

		topBar.Append(sortButtons);

		float sortButtonsRight = sortButtons.GetDimensions().Width + padding;

		recipeButtons = new UIButtonChoice(new Asset<Texture2D>[]
				{
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/RecipeAvailable"),
					MagicStorage.Instance.Assets.Request<Texture2D>("Assets/RecipeAll")
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.Common.RecipeAvailable"),
					Language.GetText("Mods.MagicStorage.Common.RecipeAll")
				});

		recipeButtons.OnClick += ClickRecipeButtons;
		float recipeButtonsLeft = sortButtonsRight + 32f + 3 * padding;
		recipeButtons.Left.Pixels = recipeButtonsLeft;
		topBar.Append(recipeButtons);
		float recipeButtonsRight = recipeButtonsLeft + recipeButtons.GetDimensions().Width + padding;

		searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchName"));
		searchBar.Left.Set(recipeButtonsRight + padding, 0f);
		searchBar.Width.Set(-recipeButtonsRight - 2 * padding, 1f);
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

		filterButtons.OnClick += ClickFilterButtons;

		topBar2.Append(filterButtons);

		float filterButtonsRight = filterButtons.GetDimensions().Width + padding;

		searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchMod"));
		searchBar2.Left.Set(filterButtonsRight + padding, 0f);
		searchBar2.Width.Set(-filterButtonsRight - 2 * padding, 1f);
		searchBar2.Height.Percent = 1f;
		topBar2.Append(searchBar2);

		UIText stationText = new UIText(Language.GetText("Mods.MagicStorage.Common.CraftingStations"));
		stationText.Top.Pixels = 76f;
		panel.Append(stationText);

		stationZone = new UISlotZone(GetStation, inventoryScale);
		stationZone.Top.Pixels = 100f;
		stationZone.OnMouseDown += PressStation;
		panel.Append(stationZone);

		UIScrollableBar scrollbar = new UIScrollableBar();
		stationZone.Scrollbar = scrollbar;

		stationZone.SetDimensions(columns);

		UIScrollButton stationButtonLeft = new UIScrollButton(TextureAssets.ScrollLeftButton, false);
		stationButtonLeft.Top.Pixels = stationZone.Top.Pixels + stationZone.padding;
		stationButtonLeft.Left.Pixels = stationZone.Left.Pixels + stationZone.Width.Pixels + 13f;
		stationButtonLeft.OnMouseDown += (_, _) =>
		{
			if (stationButtonLeft.active)
			{
				stationButtonLeft.color = StateColor.blue;
				stationZone.Scrollbar.ViewPosition -= 1f;
			}
		};
		stationButtonLeft.OnMouseUp += (_, _) =>
		{
			stationButtonLeft.color = Color.White;
		};
		panel.Append(stationButtonLeft);

		UIScrollButton stationButtonRight = new UIScrollButton(TextureAssets.ScrollRightButton);
		stationButtonRight.Top.Pixels = stationZone.Top.Pixels + stationZone.Height.Pixels - stationZone.padding - stationButtonRight.Height.Pixels;
		stationButtonRight.Left = stationButtonLeft.Left;
		stationButtonRight.OnMouseDown += (_, _) =>
		{
			if (stationButtonRight.active)
			{
				stationButtonRight.color = StateColor.blue;
				stationZone.Scrollbar.ViewPosition += 1f;
			}
		};
		stationButtonRight.OnMouseUp += (_, _) =>
		{
			stationButtonRight.color = Color.White;
		};

		panel.Append(stationButtonRight);

		stationZone.OnUpdate += (_) =>
		{
			stationButtonLeft.active = stationZone.Scrollbar.ViewPosition != 0;
			stationButtonRight.active = access.stations.Length - columns != stationZone.Scrollbar.ViewPosition;
		};

		UIText recipeText = new UIText(Language.GetText("Mods.MagicStorage.Common.Recipes"));
		recipeText.Top.Pixels = 152f;
		panel.Append(recipeText);

		recipeZone = new UISlotZone(GetRecipe, GetRecipeColor, GetRecipeSlotTexture, inventoryScale);
		recipeZone.Top.Pixels = 176f;
		recipeZone.OnMouseDown += PressRecipe;
		panel.Append(recipeZone);

		scrollbar = new UIScrollableBar();
		scrollbar.Left.Pixels = -30f;
		recipeZone.Scrollbar = scrollbar;
		panel.Append(scrollbar);

		recipeZone.SetDimensions(columns, (int)((panel.Height.Pixels - panel.PaddingBottom - recipeZone.Top.Pixels) / (slotHeight + 2 * recipeZone.padding)));

		UIText recipePanelHeader = new UIText(Language.GetText("Mods.MagicStorage.Common.SelectedRecipe"));
		recipePanel.Append(recipePanelHeader);

		UIText ingredientText = new UIText(Language.GetText("Mods.MagicStorage.Common.Ingredients"));
		ingredientText.Top.Pixels = 30f;
		recipePanel.Append(ingredientText);

		previewZone = new UISlotZone(GetSelectedItem, inventoryScale);
		previewZone.Left.Set(-slotWidth, 1f);
		recipePanel.Append(previewZone);

		ingredientZone = new UISlotZone(GetIngredient, GetIngredientColor, GetIngredientSlotTexture, smallScale);
		ingredientZone.Top.Pixels = 54f;
		ingredientZone.SetDimensions(recipePanelColumns, ingredientRows);
		ingredientZone.OnMouseDown += PressIngredient;
		recipePanel.Append(ingredientZone);

		UIText reqObjText = new UIText(Language.GetText("LegacyInterface.22"));
		reqObjText.Top.Pixels = 136f;
		recipePanel.Append(reqObjText);

		reqObjText2 = new UIText(string.Empty);
		reqObjText2.Top.Pixels = 160f;
		recipePanel.Append(reqObjText2);

		UIText storedItemsText = new UIText(Language.GetText("Mods.MagicStorage.Common.StoredItems"));
		storedItemsText.Top.Pixels = 190f;
		recipePanel.Append(storedItemsText);

		storageZone = new UISlotZone(GetStorage, smallScale);
		storageZone.Top.Pixels = 214f;
		recipePanel.Append(storageZone);

		craftButton = new UITextPanel<LocalizedText>(Language.GetText("LegacyMisc.72"), 1f);
		craftButton.Top.Set(-32f, 1f);
		craftButton.Width.Pixels = 100f;
		craftButton.Height.Pixels = 24f;
		craftButton.PaddingTop = 8f;
		craftButton.PaddingBottom = 8f;

		recipePanel.Append(craftButton);

		resultZone = new UISlotZone(GetResultItem, inventoryScale);
		resultZone.Left.Set(-slotWidth, 1f);
		resultZone.Top.Set(-slotHeight, 1f);
		resultZone.OnMouseDown += PressResult;
		recipePanel.Append(resultZone);

		float recipeHeight = 0;

		for (int k = storageRows; k > 0; k--)
		{
			storageZone.SetDimensions(recipePanelColumns, k);
			recipeHeight = storageZone.Height.Pixels + ingredientZone.Height.Pixels + 230f;

			if (recipeHeight < panel.Height.Pixels)
			{
				break;
			}
		}

		recipePanel.Height.Pixels = recipeHeight;
		recipePanel.Recalculate();

		Append(panel);
		Append(recipePanel);
	}

	private bool AnimationOpen()
	{
		const float speed = 0.2f;

		panel.Left.Pixels = Animation.Ease(-panel.Width.Pixels, panelLeft, speed, ref easeTimer);

		return Math.Abs(panel.Left.Pixels - panelLeft) > 0.1;
	}

	private bool AnimationClose()
	{
		const float speed = 0.35f;

		panel.Left.Pixels = Animation.Ease(panelLeft, -panel.Width.Pixels, speed, ref easeTimer);

		return Math.Abs(panel.Left.Pixels - (-panel.Width.Pixels)) > 0.1;
	}

	public override void OnActivate()
	{
		UISystem system = ModContent.GetInstance<UISystem>();
		player = Main.LocalPlayer;
		heart = StoragePlayer.LocalPlayer.GetStorageHeart();
		access = StoragePlayer.LocalPlayer.GetCraftingAccess();

		if (access == null || heart == null)
		{
			system.UI.SetState(null);
			return;
		}

		vanillaRecipes = Array.FindAll(Main.recipe, recipe => !recipe.createItem.IsAir);
		Array.Sort(vanillaRecipes, (a, b) => SortMode.Default.Compare(a.createItem, b.createItem));

		adjTiles = new bool[player.adjTile.Length];
		adjLiquids= new bool[Main.maxLiquidTypes];
		ingredients = new List<Item>(4);
		craftableIngredients = new List<byte>(4);
		itemCounts = new Dictionary<int, int>();
		recipeGroupCounts = new Dictionary<int, int>(RecipeGroup.recipeGroups.Count);
		conditions = new List<Recipe.Condition>(3);
		storageItems = new List<Item>();

		panel.Left.Pixels = opening ? -panel.Width.Pixels : panelLeft;

		system.inputs.Add(searchBar);
		system.inputs.Add(searchBar2);

		Refresh();
	}

	public override void OnDeactivate()
	{
		player = null;
		heart = null;
		access = null;

		easeTimer = 0;
		stackSplit = 0;
		stackDelay = 7;
		stackCounter = 0;

		vanillaRecipes = null;
		recipes = null;
		recipeAvailable = null;
		selectedRecipe = null;

		adjTiles = null;
		adjLiquids= null;
		ingredients = null;
		craftableIngredients = null;
		itemCounts = null;
		recipeGroupCounts = null;
		conditions = null;
		storageItems = null;

		recipeMode = RecipeMode.Available;
		filterMode = FilterMode.All;
		sortMode = SortMode.Default;

		filterButtons.choice = FilterMode.index(filterMode);
		sortButtons.choice = SortMode.index(sortMode);
		recipeButtons.choice = (int) RecipeMode.Available;

		stationZone.Scrollbar.ViewPosition = 0.0f;

		modFilter = string.Empty;
		nameFilter = string.Empty;

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

			return;
		}

		// TODO: Check access and heart changes

		isMouseHovering = false;
		foreach (UIElement e in Elements)
		{
			isMouseHovering |= e.IsMouseHovering;
		}

		Main.LocalPlayer.mouseInterface |= isMouseHovering;

		UpdateStackTimer();
		UpdateCraftButton();
		UpdateResultSlot();
		UpdateCursor();

		if (nameFilter != searchBar.Text || modFilter != searchBar2.Text)
		{
			nameFilter = searchBar.Text;
			modFilter = searchBar2.Text;

			RefreshRecipes();
		}
	}

	private void UpdateCraftButton()
	{
		if (selectedRecipe == null || !IsAvailable(selectedRecipe))
		{
			craftButton.BackgroundColor = StateColor.darkBlue;
			return;
		}

		if (craftButton.IsMouseHovering)
		{
			craftButton.BackgroundColor = StateColor.lightBlue;

			if (Main.mouseLeft && stackSplit <= 1)
			{
				stackSplit = stackSplit == 0 ? 15 : stackDelay;

				TryCraft();

				RefreshItems();
				RefreshStorageItems();
				RefreshRecipes();

				SoundEngine.PlaySound(SoundID.Grab);
			}
		}
		else
		{
			craftButton.BackgroundColor = StateColor.blue;
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

		if (!(PlayerInput.Triggers.Current.MouseLeft || PlayerInput.Triggers.Current.MouseRight))
		{
			stackSplit = 0;
		}
		else if (stackSplit > 0)
		{
			stackSplit--;
		}
	}

	private void UpdateResultSlot()
	{
		if (resultZone.IsMouseHovering && Main.mouseRight && stackSplit <= 1 && !resultItem.IsAir)
		{
			if (stackSplit == 0)
			{
				stackSplit = 15;
			}
			else
			{
				stackSplit = stackDelay;
			}

			Item withdraw = resultItem.Clone();
			withdraw.stack = 1;

			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = heart.Withdraw(withdraw);
			}
			else if (Main.mouseItem.type == resultItem.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
			{
				heart.Withdraw(withdraw);
				Main.mouseItem.stack += 1;
			}
			else return;

			RefreshItems();
			RefreshStorageItems();

			if (withdraw.material)
			{
				RefreshRecipes();
			}

			SoundEngine.PlaySound(SoundID.MenuTick);
		}
	}

	public void UpdateCursor()
	{
		if (ItemSlot.ShiftInUse)
		{
			if (resultZone.mouseSlot == 0 && !resultItem.IsAir || !GetStation(stationZone.mouseSlot).IsAir)
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
		if (slot >= 0 && slot < access.stations.Length)
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
		if (selectedRecipe != null && slot < ingredients.Count)
		{
			return ingredients[slot];
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

	private Color GetIngredientColor(int slot)
	{
		if (slot >= 0 && slot < craftableIngredients.Count)
		{
			return craftableIngredients[slot] switch
			{
				1  => StateColor.slotLight,
				2  => StateColor.slotRed,
				_  => StateColor.slotBG,
			};
		}

		return StateColor.slotBG;
	}

	private Texture2D GetIngredientSlotTexture(int slot)
	{
		if (slot >= 0 && slot < craftableIngredients.Count && craftableIngredients[slot] == 1)
		{
			return TextureAssets.InventoryBack16.Value;
		}

		return TextureAssets.InventoryBack13.Value;
	}

	private Color GetRecipeColor(int slot)
	{
		if (slot < recipes.Count)
		{
			if (!recipeAvailable[slot])
			{
				return recipes[slot] != selectedRecipe ? StateColor.slotRed : StateColor.darkRed;
			}

			if (recipes[slot] == selectedRecipe)
			{
				return StateColor.slotSelected;
			}
		}

		return StateColor.slotBG;
	}

	private Texture2D GetRecipeSlotTexture(int slot)
	{
		if (slot < recipes.Count && recipes[slot] == selectedRecipe)
		{
			return TextureAssets.InventoryBack15.Value;
		}

		return TextureAssets.InventoryBack13.Value;
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
		List<string> conditions;
		conditions = selectedRecipe.requiredTile
			.Select(MapHelper.TileToLookup)
			.Select(Lang.GetMapObjectName)
			.ToList();

		if (selectedRecipe.HasCondition(Recipe.Condition.NearWater))
		{
			conditions.Add(Language.GetTextValue("LegacyInterface.53"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.NearHoney))
		{
			conditions.Add(Language.GetTextValue("LegacyInterface.58"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.NearLava))
		{
			conditions.Add(Language.GetTextValue("LegacyInterface.56"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.InSnow))
		{
			conditions.Add(Language.GetTextValue("LegacyInterface.123"));
		}

		if (conditions.Count == 0)
		{
			reqObjText2.SetText(Language.GetTextValue("LegacyInterface.23"));
		}
		else
		{
			reqObjText2.SetText(string.Join(", ", conditions));
		}
	}

	public void Refresh()
	{
		RefreshStations();
		RefreshItems();
		RefreshStorageItems();
		RefreshRecipes();
	}

	private void RefreshItems()
	{
		itemCounts.Clear();

		foreach (int key in RecipeGroup.recipeGroups.Keys)
		{
			recipeGroupCounts[key] = 0;
		}

		items = ItemSorter.SortAndFilter(heart.GetStoredItems(), SortMode.Id, FilterMode.All, "", "");

		foreach (Item item in items)
		{
			if (!item.material)
			{
				continue;
			}

			if (itemCounts.ContainsKey(item.type))
			{
				itemCounts[item.type] += item.stack;
			}
			else
			{
				itemCounts[item.type] = item.stack;
			}

			foreach (var (id, recipeGroup) in RecipeGroup.recipeGroups)
			{
				if (recipeGroup.ContainsItem(item.type))
				{
					recipeGroupCounts[id] += item.stack;
				}
			}
		}

	}

	private void RefreshRecipes()
	{
		Recipe[] newRecipes = Filter(vanillaRecipes, filterMode, nameFilter, modFilter);
		if (sortMode != SortMode.Default)
		{
			Sort(newRecipes, sortMode);
		}

		if (recipeMode == RecipeMode.Available)
		{
			recipes = newRecipes.AsParallel().AsOrdered().Where(IsAvailable).ToList();
			recipeAvailable = new bool[recipes.Count];
			Array.Fill(recipeAvailable, true);
		}
		else if (recipeMode == RecipeMode.All)
		{
			recipes = newRecipes.ToList();
			recipeAvailable = newRecipes.AsParallel().AsOrdered().Select(IsAvailable).ToArray();
		}

		recipeZone.UpdateScrollBar((recipes.Count + columns - 1) / columns);
	}

	private void RefreshStations()
	{
		Array.Clear(adjTiles,   0, adjTiles.Length);
		Array.Clear(adjLiquids, 0, adjLiquids.Length);
		conditions.Clear();

		foreach (Item item in access.stations)
		{
			if (!item.IsAir && item.createTile >= 0)
			{
				ModTile tile = TileLoader.GetTile(item.createTile);
				if (tile is not null)
				{
					foreach (int t in tile.AdjTiles)
					{
						adjTiles[t] = true;
					}
				}

				adjLiquids[LiquidID.Water] |= TileID.Sets.CountsAsWaterSource[item.createTile];
				adjLiquids[LiquidID.Lava]  |= TileID.Sets.CountsAsLavaSource[item.createTile];
				adjLiquids[LiquidID.Honey] |= TileID.Sets.CountsAsHoneySource[item.createTile];

				adjTiles[TileID.Bottles]     |= item.createTile == TileID.AlchemyTable;
				adjTiles[TileID.Tables]      |= item.createTile == TileID.BewitchingTable || item.createTile == TileID.AlchemyTable || item.createTile == TileID.Tables2;
				adjTiles[TileID.Anvils]      |= item.createTile == TileID.MythrilAnvil;
				adjTiles[TileID.Furnaces]    |= item.createTile == TileID.Hellforge || item.createTile == TileID.AdamantiteForge || item.createTile == TileID.GlassKiln;
				adjTiles[TileID.WorkBenches] |= item.createTile == TileID.HeavyWorkBench;
				adjTiles[TileID.Hellforge]   |= item.createTile == TileID.AdamantiteForge;

				adjTiles[item.createTile] = true;
			}

			adjLiquids[LiquidID.Water] |= item.type == ItemID.WaterBucket || item.type == ItemID.BottomlessBucket;
			adjLiquids[LiquidID.Lava]  |= item.type == ItemID.LavaBucket;
			adjLiquids[LiquidID.Honey] |= item.type == ItemID.HoneyBucket;
		}

		if (adjLiquids[LiquidID.Water])
			conditions.Add(Recipe.Condition.NearWater);

		if (adjLiquids[LiquidID.Lava])
			conditions.Add(Recipe.Condition.NearLava);

		if (adjLiquids[LiquidID.Honey])
			conditions.Add(Recipe.Condition.NearHoney);

		stationZone.UpdateScrollBar(access.stations.Length + 1 - columns);
	}

	private bool IsAvailable(Recipe recipe)
	{
		if (recipe.requiredTile.Any(t => !adjTiles[t]))
			return false;

		if (recipe.Conditions.Any(c => !(conditions.Contains(c) || c.RecipeAvailable(recipe))))
			return false;

		return recipe.requiredItem.TrueForAll(ingredient => EnoughOf(recipe, ingredient));
	}

	private bool EnoughOf(Recipe recipe, Item item)
	{
		return itemCounts.ContainsKey(item.type) && itemCounts[item.type] >= item.stack
			|| recipe.acceptedGroups.Any(id => RecipeGroup.recipeGroups[id].ContainsItem(item.type) && recipeGroupCounts[id] >= item.stack);
	}

	private void RefreshStorageItems()
	{
		storageItems.Clear();
		resultItem = UISlotZone.Air;

		if (selectedRecipe != null)
		{
			foreach (Item item in items)
			{
				if (item.type == selectedRecipe.createItem.type)
				{
					resultItem = item;
				}

				if (!item.material)
				{
					continue;
				}

				foreach (Item ingredient in selectedRecipe.requiredItem)
				{
					if (item.type == ingredient.type || RecipeGroupMatch(selectedRecipe, ingredient.type, item.type))
					{
						storageItems.Add(item.Clone());
					}
				}
			}
		}
	}

	private bool RecipeGroupMatch(Recipe recipe, int type1, int type2)
	{
		return recipe.acceptedGroups
			.Select(id => RecipeGroup.recipeGroups[id])
			.Any(g => g.ContainsItem(type1) && g.ContainsItem(type2));
	}

	private void PressIngredient(UIEvent _event, UIElement _element)
	{
		if (selectedRecipe == null) return;

		int slot = ingredientZone.mouseSlot;

		if (slot >= 0 && slot < selectedRecipe.requiredItem.Count)
		{
			Item ingredient = selectedRecipe.requiredItem[slot];
			bool available = false;
			Recipe? recipe = FindRecipe(ingredient, out available);
			if (recipe != null)
			{
				SelectRecipe(recipe);
			}
		}
	}

	private void ClickRecipeButtons(UIEvent _event, UIElement _element)
	{
		RecipeMode newMode = (RecipeMode) recipeButtons.choice;
		if (newMode != recipeMode)
		{
			recipeMode = newMode;
			SoundEngine.PlaySound(SoundID.MenuTick);

			RefreshRecipes();
		}
	}

	private void ClickFilterButtons(UIEvent _event, UIElement _element)
	{
		IFilter<Item> newFilter = FilterMode.from(filterButtons.choice);
		if (newFilter != filterMode)
		{
			filterMode = newFilter;
			SoundEngine.PlaySound(SoundID.MenuTick);

			RefreshRecipes();
		}
	}

	private void ClickSortButtons(UIEvent _event, UIElement _element)
	{
		IComparer<Item> newSort = SortMode.from(sortButtons.choice);
		if (newSort != sortMode)
		{
			sortMode = newSort;
			SoundEngine.PlaySound(SoundID.MenuTick);

			RefreshRecipes();
		}
	}

	private void PressRecipe(UIEvent _event, UIElement _element)
	{
		int slot = recipeZone.mouseSlot;

		if (slot >= 0 && slot < recipes.Count())
		{
			Recipe recipe = recipes[slot];
			if (!recipe.createItem.IsAir)
			{
				SelectRecipe(recipe);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}

	private void PressStation(UIEvent _event, UIElement _element)
	{
		int slot = stationZone.mouseSlot;

		if (slot < 0 || slot >= access.stations.Length || access.stations[slot].IsAir && Main.mouseItem.IsAir)
		{
			return;
		}

		int oldType = access.stations[slot].type;

		if (ItemSlot.ShiftInUse)
		{
			Item station = player.GetItem(Main.myPlayer, access.stations[slot], GetItemSettings.InventoryEntityToPlayerInventorySettings);
			if (station.IsAir)
			{
				access.WithdrawStation(slot);
			}
		}
		else if (Main.mouseItem.type == access.stations[slot].type && Main.mouseItem.stack < Main.mouseItem.maxStack)
		{
			access.WithdrawStation(slot);
			Main.mouseItem.stack += 1;
		}
		else
		{
			Main.mouseItem = access.SwapStations(Main.mouseItem, slot);
		}

		if (access.stations[slot].type != oldType)
		{
			RefreshStations();
			RefreshRecipes();

			SoundEngine.PlaySound(SoundID.Grab);
		}
	}

	private void PressResult(UIEvent _event, UIElement _element)
	{
		if (selectedRecipe == null || Main.mouseItem.IsAir && resultItem.IsAir || resultZone.mouseSlot != 0) return;

		if (!Main.mouseItem.IsAir && selectedRecipe.createItem.type == Main.mouseItem.type)
		{
			heart.Deposit(Main.mouseItem);
		}
		else if (Main.mouseItem.IsAir && !resultItem.IsAir)
		{
			Item withdraw = resultItem.Clone();
			if (withdraw.stack > withdraw.maxStack)
			{
				withdraw.stack = withdraw.maxStack;
			}

			Main.mouseItem = heart.Withdraw(withdraw);

			if (ItemSlot.ShiftInUse)
			{
				Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, GetItemSettings.InventoryEntityToPlayerInventorySettings);
			}
		}

		RefreshItems();
		RefreshStorageItems();
		RefreshRecipes();

		SoundEngine.PlaySound(SoundID.Grab);
	}

	private void SelectRecipe(Recipe recipe)
	{
		if (recipe != null && selectedRecipe != recipe)
		{
			selectedRecipe = recipe;

			UpdateRecipeText();

			ingredients.Clear();
			craftableIngredients.Clear();

			foreach (Item requiredItem in selectedRecipe.requiredItem)
			{
				Item item = requiredItem;

				if (selectedRecipe.ProcessGroupsForText(item.type, out string nameOverride))
				{
					item = item.Clone();
					item.SetNameOverride(nameOverride);
				}

				ingredients.Add(item);
			}

			foreach (Item ingredient in selectedRecipe.requiredItem)
			{
				bool available = false;
				Recipe? ingredientRecipe = FindRecipe(ingredient, out available);

				if (ingredientRecipe == null)
				{
					craftableIngredients.Add(0);
				}
				else if (!EnoughOf(selectedRecipe, ingredient))
				{
					craftableIngredients.Add(available ? (byte) 1 : (byte) 2);
				}
				else
				{
					craftableIngredients.Add(0);
				}
			}

			RefreshStorageItems();
		}
	}

	private Recipe? FindRecipe(Item item, out bool available)
	{
		available = false;
		Recipe? result = null;

		foreach (Recipe recipe in vanillaRecipes)
		{
			if (recipe.createItem.type == item.type)
			{
				result = recipe;

				if (IsAvailable(result))
				{
					available = true;
					break;
				}
			}
		}

		return result;
	}

	private void TryCraft()
	{
		List<Item> toWithdraw = new List<Item>(selectedRecipe.requiredItem.Count);

		foreach (Item requiredItem in selectedRecipe.requiredItem)
		{
			if (requiredItem.stack <= 0)
			{
				continue;
			}

			int stack = requiredItem.stack;

			foreach (Item storageItem in storageItems)
			{
				if (requiredItem.type == storageItem.type || RecipeGroupMatch(selectedRecipe, requiredItem.type, storageItem.type))
				{
					if (storageItem.stack > stack)
					{
						Item temp = storageItem.Clone();
						temp.stack = stack;
						toWithdraw.Add(temp);
						storageItem.stack -= stack;

						break;
					}
					else
					{
						toWithdraw.Add(storageItem.Clone());

						stack -= storageItem.stack;

						storageItem.stack = 0;
						storageItem.type = 0;

						if (stack == 0) { break; }
					}
				}
			}
		}

		Item resultItem = selectedRecipe.createItem.Clone();
		resultItem.Prefix(-1);

		Vector.BitwiseOr(
				new Vector<byte>(Unsafe.As<byte[]>(player.adjTile)),
				new Vector<byte>(Unsafe.As<byte[]>(adjTiles)))
			.CopyTo(Unsafe.As<byte[]>(player.adjTile));

		player.alchemyTable = adjTiles[TileID.AlchemyTable];
		player.adjWater = adjLiquids[LiquidID.Water];
		player.adjLava  = adjLiquids[LiquidID.Lava];
		player.adjHoney = adjLiquids[LiquidID.Honey];

		toWithdraw.ForEach(item => RecipeLoader.ConsumeItem(selectedRecipe, item.type, ref item.stack));
		toWithdraw = toWithdraw.FindAll(item => item.stack > 0);

		RecipeLoader.OnCraft(resultItem, selectedRecipe, toWithdraw);

		foreach (Item item in Craft(toWithdraw, resultItem))
		{
			player.QuickSpawnClonedItem(new EntitySource_TileEntity(heart), item, item.stack);
		}
	}

	internal IEnumerable<Item> Craft(List<Item> toWithdraw, Item result)
	{
		List<Item> items = new List<Item>(toWithdraw.Count);
		foreach (Item tryWithdraw in toWithdraw)
		{
			Item withdrawn = heart.Withdraw(tryWithdraw);
			if (!withdrawn.IsAir)
			{
				items.Add(withdrawn);
			}

			if (withdrawn.stack < tryWithdraw.stack)
			{
				for (int k = 0; k < items.Count; k++)
				{
					heart.Deposit(items[k]);
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
		heart.Deposit(result);
		if (!result.IsAir)
		{
			items.Add(result);
		}

		return items;
	}

	public static void Sort(Recipe[] recipes, IComparer<Item> sortMode)
	{
		Array.Sort(recipes, (a, b) => sortMode.Compare(a.createItem, b.createItem));
	}

	public static Recipe[] Filter(Recipe[] recipes, IFilter<Item> filter, string nameFilter, string modFilter)
	{
		return Array.FindAll(recipes, recipe => filter.Passes(recipe.createItem) && ItemSorter.FilterName(recipe.createItem, modFilter, nameFilter));
	}
}
