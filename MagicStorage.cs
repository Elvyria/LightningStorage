global using System;
global using System.Collections.Generic;

global using Terraria;
global using Terraria.ID;
global using Terraria.Audio;
global using Terraria.ModLoader;
global using Terraria.UI;

using System.IO;

namespace MagicStorage;

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
	}
}
