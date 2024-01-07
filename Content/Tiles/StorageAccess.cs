using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using ReLogic.Content;

using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameInput;

using LightningStorage.Common.Players;
using LightningStorage.Content.TileEntities;

namespace LightningStorage.Content.Tiles;

public class StorageAccess : StorageComponent
{
	[AllowNull]
	private Asset<Texture2D> glowTexture;

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.tileLighted[Type] = true;
	}

	public override void Load()
	{
		base.Load();
		glowTexture = Mod.Assets.Request<Texture2D>("Content/Tiles/" + Name + "_Glow");
	}

	public override int ItemType(int frameX, int frameY)
	{
		return ModContent.ItemType<Items.StorageAccess>();
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		r = g = 0.15f * (MathF.Sin((float)Main.timeForVisualEffects * 0.01f - MathHelper.PiOver2) + 3f);
	}

	public virtual TEStorageHeart? GetHeart(int i, int j)
	{
		TEStorageCenter? center = TEStorageComponent.FindStorageCenter(new Point16(i, j));
		return center?.GetHeart();
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = ItemType(tile.TileFrameX, tile.TileFrameY);
		player.noThrow = 2;
	}

	public override bool RightClick(int i, int j)
	{
		(i, j) = Main.tile[i, j].FrameOrigin(i, j);

		if (GetHeart(i, j) == null)
		{
			Main.NewText("This access is not connected to a Storage Heart!");
			return true;
		}

		if (PlayerInput.GrappleAndInteractAreShared)
		{
			PlayerInput.Triggers.JustPressed.Grapple = false;
		}

		StoragePlayer modPlayer = Main.LocalPlayer.GetModPlayer<StoragePlayer>();

		Point16 toOpen = new Point16(i, j);
		Point16 prevOpen = modPlayer.ViewingStorage();
		if (prevOpen == toOpen)
		{
			modPlayer.CloseStorage();
		}
		else
		{
			modPlayer.OpenStorage(toOpen);
		}

		return true;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Vector2 drawPos = zero + 16f * new Vector2(i, j) - Main.screenPosition;
		Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		Color lightColor = Lighting.GetColor(i, j, Color.White);
		Color color = Color.Lerp(lightColor, Color.White, Main.essScale);
		spriteBatch.Draw(glowTexture.Value, drawPos, frame, color);
	}
}
