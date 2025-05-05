using System;

using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class Cookie : Sprite
{
	static SKSvg s_cookie;
	static SKSvg s_cookieBite;
	static SKSvg s_cookieBiteBite;
	static SKSvg s_cookieBiteBiteBite;

	static Cookie()
	{
		s_cookie = LoadSVGResource("cookie.svg");
		s_cookieBite = LoadSVGResource("cookie bite.svg");
		s_cookieBiteBite = LoadSVGResource("cookie bite bite.svg");
		s_cookieBiteBiteBite = LoadSVGResource("cookie bite bite bite.svg");
	}

	float s_duration = 48 / 23.976f;

	public override float MaxWidth => 189;
	public override float MaxHeight => 103.6f;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		int frameNumber = (int)Math.Round(t * 23.976f);

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		{
			if (frameNumber < 12)
				target.DrawPicture(s_cookie.Picture);
			else if (frameNumber < 24)
				target.DrawPicture(s_cookieBite.Picture);
			else if (frameNumber < 36)
				target.DrawPicture(s_cookieBiteBite.Picture);
			else
				target.DrawPicture(s_cookieBiteBiteBite.Picture);
		}
	}
}
