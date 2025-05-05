using System;

using SkiaSharp;

using Svg.Skia;

using Utility;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class FingerMonster : Sprite
{
	static SKSvg s_fingerMonster;

	static FingerMonster()
	{
		s_fingerMonster = LoadSVGResource("finger monster.svg");
	}

	Path s_path = new Path(
		[
			new PathPoint(0, new SKPoint(167, 45), true),
			new PathPoint(6, new SKPoint(120, 0), true),
			new PathPoint(13, new SKPoint(79, 46), false),
			new PathPoint(19, new SKPoint(41, 10), true),
			new PathPoint(25, new SKPoint(0, 48), false),
			new PathPoint(36, new SKPoint(90, 86), false),
			new PathPoint(47, new SKPoint(167, 45), false),
		]);

	float s_duration = 48 / 23.976f;

	public override float MaxWidth => 256;
	public override float MaxHeight => 180;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		int frameNumber = (int)Math.Round(t * 23.976f);

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Scale(0.75f, 0.75f))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		using (transform.Translate(s_path.GetTranslationAtFrame(frameNumber)))
			target.DrawPicture(s_fingerMonster.Picture);
	}
}
