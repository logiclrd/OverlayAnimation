using System;

using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class HeartStaker : Sprite
{
	static SKSvg s_heartStaker;
	static SKSvg s_heartStakerBang;
	static SKSvg s_heartStakerLeft;
	static SKSvg s_heartStakerRight;

	static HeartStaker()
	{
		s_heartStaker = LoadSVGResource("heart staker.svg");
		s_heartStakerLeft = LoadSVGResource("heart staker left.svg");
		s_heartStakerRight = LoadSVGResource("heart staker right.svg");
		s_heartStakerBang = LoadSVGResource("heart staker bang.svg");
	}

	SKPoint BangOffset = new SKPoint(31, 0);

	public override float MaxWidth => 190;
	public override float MaxHeight => 115;

	const int Frame1_DurationFrames = 32;
	const int Frame2_DurationFrames = 3;
	const int Frame3_DurationFrames = 35;

	const int DurationFrames = Frame1_DurationFrames + Frame2_DurationFrames + Frame3_DurationFrames;
	const float DurationSeconds = DurationFrames / 23.976f;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t > DurationSeconds)
			t -= DurationSeconds;

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		using (transform.Translate(10, 0))
		{
			int frameNumber = (int)Math.Round(t * DurationFrames / DurationSeconds);

			int frame1End = Frame1_DurationFrames;
			int frame2End = frame1End + Frame2_DurationFrames;
			int frame3End = frame2End + Frame3_DurationFrames;

			if (frameNumber < frame1End)
				target.DrawPicture(s_heartStaker.Picture);
			else if (frameNumber < frame2End)
			{
				target.DrawPicture(s_heartStaker.Picture);
				target.DrawPicture(s_heartStakerBang.Picture);
			}
			else
			{
				using (transform.Translate(75, 100))
				using (transform.RotateDegrees(-7))
				using (transform.Translate(-75, -100))
				using (transform.Translate(-3, 5))
					target.DrawPicture(s_heartStakerLeft.Picture);

				using (transform.Translate(96, 100))
				using (transform.RotateDegrees(5))
				using (transform.Translate(-96, -100))
				using (transform.Translate(3, 5))
					target.DrawPicture(s_heartStakerRight.Picture);
			}
		}
	}
}