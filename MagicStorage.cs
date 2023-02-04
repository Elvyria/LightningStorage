using System.IO;

using Terraria.ModLoader;

namespace MagicStorage
{
	public class MagicStorage : Mod
	{
		public static MagicStorage Instance;

		public override void Load()
		{
			Instance = this;
		}

		public override void Unload()
		{
			Instance = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetHelper.HandlePacket(reader, whoAmI);
		}
	}
}

