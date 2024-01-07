global using LightningStorage.Common.Extensions;

using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LightningStorage.Common.Extensions;

public static class TagExtensions
{
	public static TagCompound BuildTag(Point16 p)
	{
		return new TagCompound() {{ "X", p.X }, { "Y", p.Y }};
	}

	public static void SetPoint16(this TagCompound tag, string key, Point16 p)
	{
		tag.Set(key, BuildTag(p));
	}

	public static Point16 GetPoint16(this TagCompound tag, string key)
	{
		TagCompound innerTag = tag.GetCompound(key);
		return innerTag.GetPoint16();
	}

	public static Point16 GetPoint16(this TagCompound tag)
	{
		return new Point16(tag.GetShort("X"), tag.GetShort("Y"));
	}
}
