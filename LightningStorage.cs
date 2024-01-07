global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;

global using Terraria;
global using Terraria.ID;
global using Terraria.Audio;
global using Terraria.ModLoader;
global using Terraria.UI;

using System.IO;

namespace LightningStorage;

public class LightningStorage : Mod
{
	[AllowNull]
	public static LightningStorage Instance;

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
