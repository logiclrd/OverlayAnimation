using System;

using SkiaSharp;

using Svg.Skia;

using Utility;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class Moustache : Sprite
{
	static SKSvg s_moustache;

	static Moustache()
	{
		s_moustache = LoadSVGResource("moustache.svg");
	}

	static float s_duration = 24 / 23.976f;

	static float s_rotationDegrees = 12;

	public override float MaxWidth => 190f;
	public override float MaxHeight => 90f;

	const float ImageWidth = 180;
	const float ImageHeight = 55.7f;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		t /= s_duration;

		t *= 2;

		if (t > 1)
			t = 2 - t;

		var origin = new SKPoint(MaxWidth / 2, MaxHeight / 2);

		using (var transform = TransformScope.Translate(target, position))
		//using (transform.Translate(MaxWidth - origin / 2, (MaxHeight - origin.Y) / 2))
		using (transform.RotateDegrees(-s_rotationDegrees + 2 * s_rotationDegrees * t))
		using (transform.Translate(-ImageWidth / 2, -ImageHeight / 2))
			target.DrawPicture(s_moustache.Picture);
	}
}
