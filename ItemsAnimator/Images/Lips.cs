using System;

using SkiaSharp;

using Svg.Skia;

using Utility;

namespace ItemsAnimator.Images;

using ItemsAnimator.Utility;

public class Lips : Image
{
	static SKSvg s_lipsTop;
	static SKSvg s_lipsBottom;
	static SKSvg s_lipsTongue;

	static Lips()
	{
		s_lipsBottom = LoadSVGResource("lips bottom.svg");
		s_lipsTongue = LoadSVGResource("lips tongue.svg");
		s_lipsTop = LoadSVGResource("lips top.svg");
	}

	const int LoopFrames = 60;

	const int LickStartFrame = 26;
	const int LickEndFrame = 47;

	const float LoopSeconds = LoopFrames / 23.976f;

	static Path s_tonguePath = new Path(
		[
			new PathPoint(0, new SKPoint(43.5f, 39.4f), true),
			new PathPoint(7, new SKPoint(66.5f, 17.3f), true),
			new PathPoint(15, new SKPoint(83.4f, 19.9f), true),
			new PathPoint(19, new SKPoint(95.4f, 23.2f), true),
			new PathPoint(23, new SKPoint(96.0f, 36.4f), true),
		]);

	static Path s_tongueRotation = new Path(
		[
			new PathPoint(0, -60, true),
			new PathPoint(7, -25, true),
			new PathPoint(15, 10, true),
			new PathPoint(23, 40, true),
		]);

	SKPoint s_tongueRotationOrigin = new SKPoint(17.2f, 45.4f);

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= LoopSeconds)
			t -= LoopSeconds;

		int frameNumber = (int)Math.Round(t * 23.976f);

		target.DrawPicture(s_lipsTop.Picture, position);

		using (var transform = TransformScope.Translate(target, -92f, -46.7f))
		using (transform.Translate(s_tonguePath.GetTranslationAtFrame(frameNumber)))
		using (transform.Translate(s_tongueRotationOrigin))
		using (transform.RotateDegrees(s_tongueRotation.GetValueAtFrame(frameNumber)))
		using (transform.Translate(PointMath.Negative(s_tongueRotationOrigin)))
			target.DrawPicture(s_lipsTongue.Picture, position);

		target.DrawPicture(s_lipsBottom.Picture, position);
	}
}