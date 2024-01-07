using Microsoft.Xna.Framework;

namespace LightningStorage.Common.UI;

public static class Animation
{
	public static float Ease(float from, float to, float speed, ref float time)
	{
		const float limit = 100f;

		float sign = unchecked((float)(1 - (0xFFFFFE & (uint) (to - from) >> 31)));

		time += sign * speed * (limit - time + 2);

		return MathHelper.Lerp(from, to, time / (limit + 2));
	}

	public static float Ease(float from, float to, float speed, float time)
	{
		const float limit = 100f;

		return MathHelper.Lerp(from, to, time / (limit + 2));
	}
}
