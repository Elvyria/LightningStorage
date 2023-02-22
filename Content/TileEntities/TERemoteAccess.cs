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

	public override TEStorageHeart GetHeart()
	{
		if (locator.X < 0 || locator.Y < 0 || !ByPosition.ContainsKey(locator))
		{
			return null;
		}
		return ByPosition[locator] as TEStorageHeart;
	}

	public bool TryLocate(Point16 toLocate, out string message)
	{
		if (locator.X >= 0 && locator.Y >= 0)
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
		TEStorageHeart heart = GetHeart();
		if (heart != null && !heart.remoteAccesses.Contains(Position))
		{
			heart.remoteAccesses.Add(Position);
		}
	}

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag);

		TagCompound tagLocator = new TagCompound();
		tagLocator.Set("X", locator.X);
		tagLocator.Set("Y", locator.Y);

		tag.Set("Locator", tagLocator);
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag);
		TagCompound tagLocator = tag.GetCompound("Locator");
		locator = new Point16(tagLocator.GetShort("X"), tagLocator.GetShort("Y"));
	}

}
