global using MagicStorage.Common.Extensions;

namespace MagicStorage.Common.Extensions;

public static class TileExtensions
{
	// TODO: Better name, a bit more generic implementation
	public static (int, int) FrameOrigin(this Tile tile, int i, int j)
	{
		if (tile.TileFrameX % 36 == 18)
		{
			i--;
		}

		if (tile.TileFrameY % 36 == 18)
		{
			j--;
		}

		return (i, j);
	}
}
