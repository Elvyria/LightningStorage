using Microsoft.Xna.Framework;

namespace MagicStorage.Common;

public static class StateColor
{
	public static readonly Color lightBlue = new Color(73, 94, 171);
	public static readonly Color blue      = new Color(63, 82, 151) * 0.7f;
	public static readonly Color darkBlue  = new Color(30, 40, 100) * 0.7f;
	public static readonly Color darkRed   = new Color(255, 75, 45);

	public static readonly Color slotBG       = new Color(70, 88, 166) * 0.9f;
	public static readonly Color slotRed      = new Color(126, 50, 50) * 0.9f;
	public static readonly Color slotLight    = new Color(80, 112, 155);
	public static readonly Color slotSelected = new Color(255, 205, 255);
}
