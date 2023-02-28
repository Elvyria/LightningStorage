using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace MagicStorage.Common.UI;

class CraftingGUI : UIState
{
	private bool isMouseHovering;

	private const float inventoryScale = 0.8f;
	private const float smallScale = 0.7f;

	private const float padding = 4f;

	private const int columns = 10;
	private const int columns2 = 7;

	private const int recipePanelColumns = 7;
	private const int ingredientRows = 2;
	private const int storageRows = 4;

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

	private Dictionary<int, int> itemCounts;
	private Dictionary<int, int> recipeGroupCounts;

	private Item resultItem = UISlotZone.Air;

	private Player player;
	private TEStorageHeart heart;
	private TECraftingAccess access;

	private bool[] adjTiles;
	private bool[] adjLiquids;
	private List<Recipe.Condition> conditions;

	private List<Recipe> recipes;

	private List<bool> recipeAvailable;

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

	private static readonly Color lightBlue = new Color(73, 94, 171);
	private static readonly Color blue = new Color(63, 82, 151) * 0.7f;
	private static readonly Color darkBlue = new Color(30, 40, 100) * 0.7f;
	private static readonly Color deepBlue = new Color(163, 104, 176) * 1.15f;

	public override void OnInitialize()
	{
		float slotWidth = TextureAssets.InventoryBack9.Width() * inventoryScale;
		float slotHeight = TextureAssets.InventoryBack9.Height() * inventoryScale;
		float smallSlotWidth = TextureAssets.InventoryBack9.Width() * smallScale;
		float smallSlotHeight = TextureAssets.InventoryBack9.Height() * smallScale;

		float panelTop = Main.instance.invBottom + 60f;
		const float panelLeft = 20f;
		const float pannelBottom = 20f;

		panel = new UIPanel();

		float innerPanelLeft = panelLeft + panel.PaddingLeft;
		float innerPanelWidth = columns * (slotWidth + padding) + 20f + padding;
		float panelWidth = panel.PaddingLeft + innerPanelWidth + panel.PaddingRight;
		float panelHeight = Main.screenHeight - panelTop - pannelBottom;

		panel.Left.Set(panelLeft, 0f);
		panel.Top.Set(panelTop, 0f);
		panel.Width.Set(panelWidth, 0f);
		panel.Height.Set(panelHeight, 0f);
		panel.Recalculate();

		recipePanel = new UIPanel();
		float recipeLeft = panelLeft + panelWidth;
		float recipeWidth = columns2 * (smallSlotWidth + padding);
		recipeWidth += recipePanel.PaddingLeft + recipePanel.PaddingRight;
		recipePanel.Top = panel.Top;
		recipePanel.Left.Set(recipeLeft, 0f);
		recipePanel.Width.Set(recipeWidth, 0f);

		UIElement topBar = new UIElement();
		topBar.Width.Set(0f, 1f);
		topBar.Height.Set(32f, 0f);
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
		recipeButtons.Left.Set(recipeButtonsLeft, 0f);
		topBar.Append(recipeButtons);
		float recipeButtonsRight = recipeButtonsLeft + recipeButtons.GetDimensions().Width + padding;

		searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchName"));
		searchBar.Left.Set(recipeButtonsRight + padding, 0f);
		searchBar.Width.Set(-recipeButtonsRight - 2 * padding, 1f);
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

		filterButtons.OnClick += ClickFilterButtons;

		topBar2.Append(filterButtons);

		float filterButtonsRight = filterButtons.GetDimensions().Width + padding;

		searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.Common.SearchMod"));
		searchBar2.Left.Set(filterButtonsRight + padding, 0f);
		searchBar2.Width.Set(-filterButtonsRight - 2 * padding, 1f);
		searchBar2.Height.Set(0f, 1f);
		topBar2.Append(searchBar2);

		UIText stationText = new UIText(Language.GetText("Mods.MagicStorage.Common.CraftingStations"));
		stationText.Top.Set(76f, 0f);
		panel.Append(stationText);

		stationZone = new UISlotZone(GetStation, inventoryScale);
		stationZone.Top.Set(100f, 0f);
		stationZone.SetDimensions(columns, 1);
		stationZone.OnMouseDown += PressStation;
		panel.Append(stationZone);

		UIText recipeText = new UIText(Language.GetText("Mods.MagicStorage.Common.Recipes"));
		recipeText.Top.Set(152f, 0f);
		panel.Append(recipeText);

		recipeZone = new UISlotZone(GetRecipe, GetRecipeColor, inventoryScale);
		recipeZone.Top.Set(176f, 0f);
		recipeZone.OnMouseDown += PressRecipe;
		panel.Append(recipeZone);

		UIScrollableBar scrollbar = new UIScrollableBar();
		scrollbar.Left.Set(10, 0);
		recipeZone.SetScrollbar(scrollbar);
		panel.Append(scrollbar);

		recipeZone.SetDimensions(columns, (int)((panel.Height.Pixels - panel.PaddingBottom - recipeZone.Top.Pixels) / (slotHeight + 2 * recipeZone.padding)));

		UIText recipePanelHeader = new UIText(Language.GetText("Mods.MagicStorage.Common.SelectedRecipe"));
		recipePanel.Append(recipePanelHeader);

		UIText ingredientText = new UIText(Language.GetText("Mods.MagicStorage.Common.Ingredients"));
		ingredientText.Top.Set(30f, 0f);
		recipePanel.Append(ingredientText);

		previewZone = new UISlotZone(GetSelectedItem, inventoryScale);
		previewZone.Top.Set(0f, 0f);
		previewZone.Left.Set(-slotWidth, 1f);
		recipePanel.Append(previewZone);

		ingredientZone = new UISlotZone(GetIngredient, smallScale);
		ingredientZone.Top.Set(54f, 0f);
		ingredientZone.SetDimensions(recipePanelColumns, ingredientRows);
		ingredientZone.OnMouseDown += PressIngredient;
		recipePanel.Append(ingredientZone);

		UIText reqObjText = new UIText(Language.GetText("LegacyInterface.22"));
		reqObjText.Top.Set(136f, 0f);
		recipePanel.Append(reqObjText);

		reqObjText2 = new UIText(string.Empty);
		reqObjText2.Top.Set(160f, 0f);
		recipePanel.Append(reqObjText2);

		UIText storedItemsText = new UIText(Language.GetText("Mods.MagicStorage.Common.StoredItems"));
		storedItemsText.Top.Set(190f, 0f);
		recipePanel.Append(storedItemsText);

		storageZone = new UISlotZone(GetStorage, smallScale);
		storageZone.Top.Set(214f, 0f);
		recipePanel.Append(storageZone);

		craftButton = new UITextPanel<LocalizedText>(Language.GetText("LegacyMisc.72"), 1f);
		craftButton.Top.Set(-32f, 1f);
		craftButton.Width.Set(100f, 0f);
		craftButton.Height.Set(24f, 0f);
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

		recipePanel.Height.Set(recipeHeight, 0f);
		recipePanel.Recalculate();

		Append(panel);
		Append(recipePanel);
	}

	// public override void Recalculate()
	// {

	// base.Recalculate();
	// }

	public override void OnActivate()
	{
		player = Main.LocalPlayer;
		heart = StoragePlayer.LocalPlayer.GetStorageHeart();
		access = StoragePlayer.LocalPlayer.GetCraftingAccess();

		if (access == null || heart == null)
		{
			Deactivate();
			return;
		}

		adjTiles = new bool[player.adjTile.Length];
		adjLiquids= new bool[Main.maxLiquidTypes];
		ingredients = new List<Item>(4);
		itemCounts = new Dictionary<int, int>();
		recipeGroupCounts = new Dictionary<int, int>(RecipeGroup.recipeGroups.Count);
		conditions = new List<Recipe.Condition>(3);
		storageItems = new List<Item>();

		UISystem system = ModContent.GetInstance<UISystem>();
		system.inputs.Add(searchBar);
		system.inputs.Add(searchBar2);

		Refresh();
	}

	public override void OnDeactivate()
	{
		player = null;
		heart = null;
		access = null;

		recipes = null;
		recipeAvailable = null;
		selectedRecipe = null;

		adjTiles = null;
		adjLiquids= null;
		ingredients = null;
		itemCounts = null;
		recipeGroupCounts = null;
		conditions = null;
		storageItems = null;

		recipeMode = RecipeMode.Available;
		filterMode = FilterMode.All;
		sortMode = SortMode.Default;

		filterButtons.choice = FilterMode.All.index();
		sortButtons.choice = SortMode.Default.index();
		recipeButtons.choice = (int) RecipeMode.Available;

		modFilter = string.Empty;
		nameFilter = string.Empty;

		UISystem system = ModContent.GetInstance<UISystem>();
		system.inputs.Remove(searchBar);
		system.inputs.Remove(searchBar2);
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
				Refresh();
				SoundEngine.PlaySound(SoundID.Grab);
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

	private Color GetRecipeColor(int slot)
	{
		if (slot < recipes.Count)
		{
			if (recipes[slot] == selectedRecipe)
			{
				return Color.Lime;
			}

			if (!recipeAvailable[slot])
			{
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
		StringBuilder builder = new StringBuilder();

		for (int k = 0; k < selectedRecipe.requiredTile.Count; k++)
		{
			if (selectedRecipe.requiredTile[k] == -1)
			{
				break;
			}

			if (builder.Length != 0) builder.Append(", ");

			builder.Append(Lang.GetMapObjectName(MapHelper.TileToLookup(selectedRecipe.requiredTile[k], 0)));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.NearWater))
		{
			if (builder.Length != 0) builder.Append(", ");

			builder.Append(Language.GetTextValue("LegacyInterface.53"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.NearHoney))
		{
			if (builder.Length != 0) builder.Append(", ");

			builder.Append(Language.GetTextValue("LegacyInterface.58"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.NearLava))
		{
			if (builder.Length != 0) builder.Append(", ");

			builder.Append(Language.GetTextValue("LegacyInterface.56"));
		}

		if (selectedRecipe.HasCondition(Recipe.Condition.InSnow))
		{
			if (builder.Length != 0) builder.Append(", ");

			builder.Append(Language.GetTextValue("LegacyInterface.123"));
		}

		if (builder.Length == 0)
		{
			reqObjText2.SetText(Language.GetTextValue("LegacyInterface.23"));
		}
		else
		{
			reqObjText2.SetText(builder.ToString());
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
		Recipe[] temp = ItemSorter.SortAndFilter(Main.recipe, sortMode, filterMode, modFilter, nameFilter);

		if (recipeMode == RecipeMode.Available)
		{
			recipes = temp.AsParallel().AsOrdered().Where(recipe => IsAvailable(recipe)).ToList();
			recipeAvailable = temp.Select(recipe => true).ToList();
		}
		else if (recipeMode == RecipeMode.All)
		{
			recipes = temp.ToList();
			recipeAvailable = temp.AsParallel().AsOrdered().Select(recipe => IsAvailable(recipe)).ToList();
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
			if (item.createTile >= 0)
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
				adjTiles[TileID.Furnaces]    |= item.createTile == TileID.Hellforge || item.createTile == TileID.Hellforge || item.createTile == TileID.GlassKiln;
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
	}

	private bool IsAvailable(Recipe recipe)
	{
		if (recipe.requiredTile.Any(t => !adjTiles[t]))
			return false;

		if (recipe.Conditions.Any(c => !(conditions.Contains(c) || c.RecipeAvailable(recipe))))
			return false;

		foreach (Item ingredient in recipe.requiredItem)
		{
			if (itemCounts.ContainsKey(ingredient.type) && itemCounts[ingredient.type] >= ingredient.stack)
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
					if (ingredient.type == 0)
					{
						break;
					}
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
		if (selectedRecipe == null)
		{
			return;
		}

		int slot = ingredientZone.mouseSlot;

		if (slot >= 0 && slot < selectedRecipe.requiredItem.Count)
		{
			Item ingredient = selectedRecipe.requiredItem[slot];
			foreach (Recipe recipe in Main.recipe)
			{
				if (recipe.createItem.type == ingredient.type)
				{
					SelectRecipe(recipe);
					return;
				}
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
		if (selectedRecipe == null || Main.mouseItem.IsAir && resultItem.IsAir || resultZone.mouseSlot != 0)
		{
			return;
		}

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

		Refresh();

		SoundEngine.PlaySound(SoundID.Grab);
	}

	private void SelectRecipe(Recipe recipe)
	{
		if (recipe != null && selectedRecipe != recipe)
		{
			selectedRecipe = recipe;

			UpdateRecipeText();

			ingredients.Clear();

			foreach (Item requiredItem in selectedRecipe.requiredItem)
			{
				Item item = requiredItem.Clone();
				ingredients.Add(item);

				if (selectedRecipe.ProcessGroupsForText(item.type, out string nameOverride))
				{
					item.SetNameOverride(nameOverride);
				}
			}

			RefreshStorageItems();
		}
	}

	private void TryCraft()
	{
		List<Item> availableItems = storageItems.Select(item => item.Clone()).ToList();
		List<Item> toWithdraw = new List<Item>(selectedRecipe.requiredItem.Count);

		foreach (Item requiredItem in selectedRecipe.requiredItem)
		{
			if (requiredItem.stack <= 0)
			{
				continue;
			}

			int stack = requiredItem.stack;

			foreach (Item storageItem in availableItems)
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

	internal List<Item> Craft(List<Item> toWithdraw, Item result)
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
}
