using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Images;

using System;
using ItemsAnimator.Utility;

public class Noisemaker : Image
{
	static SKSvg s_noisemaker;
	static SKSvg s_noisemakerNootNoot;

	static Noisemaker()
	{
		s_noisemaker = LoadSVGResource("noisemaker.svg");
		s_noisemakerNootNoot = LoadSVGResource("noisemaker noot noot.svg");
	}

	float s_duration = 48 / 23.976f;

	const int Noot1Start = 7;
	const int Noot1Length = 3;
	const int Noot2Start = 12;
	const int Noot2Length = 5;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		int frameNumber = (int)Math.Round(t * 23.976f);

		using (var transform = TransformScope.Translate(target, -101.5f, -57.5f))
		{
			using (transform.Translate(0, 45))
				target.DrawPicture(s_noisemaker.Picture, position);

			if (((frameNumber >= Noot1Start) && (frameNumber - Noot1Start < Noot1Length))
			 || ((frameNumber >= Noot2Start) && (frameNumber - Noot2Start < Noot2Length)))
			{
				using (transform.Translate(112, 0))
					target.DrawPicture(s_noisemakerNootNoot.Picture, position);
			}
		}
	}
}
