using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using LightningStorage.Content.Tiles;

namespace LightningStorage.Content.TileEntities;

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
		if (locator != Point16.NegativeOne && ByPosition.ContainsKey(locator) && ByPosition[locator] is TEStorageHeart heart)
		{
			return heart;
		}

		return null;
	}

	public void Reset()
	{
		locator = Point16.NegativeOne;
	}

	public bool Bind(Point16 pos)
	{
		if (pos != Point16.NegativeOne && TileEntity.ByPosition.ContainsKey(pos) && TileEntity.ByPosition[pos] is TEStorageHeart)
		{
			locator = pos;

			TEStorageHeart? heart = GetHeart();
			if (heart != null && !heart.remoteAccesses.Contains(Position))
			{
				heart.remoteAccesses.Add(Position);
			}

			return true;
		}

		return false;
	}

    public override void OnKill()
    {
        base.OnKill();

		TEStorageHeart? heart = GetHeart();
		heart?.remoteAccesses.Remove(Position);
    }

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag);
		tag.SetPoint16("Locator", locator);
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag);
		Bind(tag.GetPoint16("Locator"));
	}
}
