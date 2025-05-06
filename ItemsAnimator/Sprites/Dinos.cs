using System;

using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class Dinos : Sprite
{
	static SKSvg s_dinos1;
	static SKSvg s_dinos2;
	static SKSvg s_dinos3;
	static SKSvg s_dinos4;

	static Dinos()
	{
		s_dinos1 = LoadSVGResource("dinos 1.svg");
		s_dinos2 = LoadSVGResource("dinos 2.svg");
		s_dinos3 = LoadSVGResource("dinos 3.svg");
		s_dinos4 = LoadSVGResource("dinos 4.svg");
	}

	float s_duration = 48 / 23.976f;

	public override float MaxWidth => 170;
	public override float MaxHeight => 140;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		int frameNumber = (int)Math.Round(t * 23.976f);

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		{
			if (frameNumber < 10)
				target.DrawPicture(s_dinos1.Picture);
			else if (frameNumber < 20)
				target.DrawPicture(s_dinos2.Picture);
			else if (frameNumber < 30)
				target.DrawPicture(s_dinos3.Picture);
			else
				target.DrawPicture(s_dinos4.Picture);
		}
	}
}
