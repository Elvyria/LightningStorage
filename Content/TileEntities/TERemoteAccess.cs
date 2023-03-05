using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using MagicStorage.Content.Tiles;

namespace MagicStorage.Content.TileEntities;

public class TERemoteAccess : TEStorageCenter
{
	private Point16 locator = Point16.NegativeOne;

	public override bool IsTileValidForEntity(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.HasTile && tile.TileType == ModContent.TileType<RemoteAccess>();
	}

	public override TEStorageHeart? GetHeart()
	{
		if (locator != Point16.NegativeOne && !ByPosition.ContainsKey(locator) && ByPosition[locator] is TEStorageHeart heart)
		{
			return heart;
		}

		return null;
	}

	public bool TryLocate(Point16 toLocate, out string message)
	{
		if (locator != Point16.NegativeOne)
		{
			message = "This Access already has a locator, please mine then replace to reset it";
			return false;
		}

		if (toLocate.X < 0 || toLocate.Y < 0)
		{
			message = "The locator has not been set to a destination";
			return false;
		}

		message = "Success!";
		locator = toLocate;
		return true;
	}

	public override void Update()
	{
		TEStorageHeart? heart = GetHeart();
		if (heart != null && !heart.remoteAccesses.Contains(Position))
		{
			heart.remoteAccesses.Add(Position);
		}
	}

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag);
		tag.SetPoint16("Locator", locator);
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag);
		locator = tag.GetPoint16("Locator");
	}

}
