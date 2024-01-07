using Terraria.GameContent.UI.Elements;

namespace LightningStorage.Common.UI;

public class UIScrollableBar : UIScrollbar
{
	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		ViewPosition -= Math.Sign(evt.ScrollWheelValue);
	}
}
