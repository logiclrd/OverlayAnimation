using System;

using SkiaSharp;

using Svg.Skia;

using Utility;

namespace ItemsAnimator.Images;

using ItemsAnimator.Utility;

public class Moustache : Image
{
	static SKSvg s_moustache;

	static Moustache()
	{
		s_moustache = LoadSVGResource("moustache.svg");
	}

	static float s_duration = 24 / 23.976f;

	static float s_rotationDegrees = 12;

	static SKPoint s_origin = new SKPoint(90f, 25f);

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		t /= s_duration;

		t *= 2;

		if (t > 1)
			t = 2 - t;

		using (var transform = TransformScope.Translate(target, -s_origin.X / 2, -s_origin.Y / 2))
		using (transform.RotateDegrees( -s_rotationDegrees + 2 * s_rotationDegrees * t))
		using (transform.Translate(new SKPoint(-s_origin.X, -s_origin.Y)))
			target.DrawPicture(s_moustache.Picture, position);
	}
}
