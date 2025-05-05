using System;

using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class Tealight : Sprite
{
	static SKSvg s_tealightBase;
	static SKSvg s_tealightFlame;

	static Tealight()
	{
		s_tealightBase = LoadSVGResource("tealight base.svg");
		s_tealightFlame = LoadSVGResource("tealight flame.svg");
	}

	float s_duration = 48 / 23.976f;

	float s_pendulumMaxAngle = 2.65f;
	float s_pendulumLength = 1500;

	float s_flameBlownAngle = 22;

	float s_scale = 0.2f;

	public override float MaxWidth => 245;
	public override float MaxHeight => 170;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= s_duration)
			t -= s_duration;

		int frameNumber = (int)Math.Round(t * 23.976f);

		t = frameNumber / 48f;

		using (var transform = new TransformScope(target))
		{
			for (int layer = 3; layer >= 0; layer--)
			{
				double cycleOffset = t * 2 * Math.PI + layer * 2;

				float angle = s_pendulumMaxAngle * (float)Math.Cos(cycleOffset);

				float fromAngle = s_pendulumMaxAngle * (float)Math.Cos(cycleOffset - 0.02);

				int direction = Math.Sign(angle - fromAngle);

				using (transform.Translate(position))
				using (transform.Translate(0, -130))
				using (transform.Scale(4 / (layer + 4f), 4 / (layer + 4f)))
				using (transform.Translate(0, 150))
				using (transform.Translate(0, s_pendulumLength))
				using (transform.RotateDegrees(angle))
				using (transform.Translate(0, -s_pendulumLength))
				using (transform.Scale(s_scale, s_scale))
				using (transform.Translate(-240, -260))
					target.DrawPicture(s_tealightBase.Picture);

				using (transform.Translate(position))
				using (transform.Translate(0, -130))
				using (transform.Scale(4 / (layer + 4f), 4 / (layer + 4f)))
				using (transform.Translate(0, 150))
				using (transform.Translate(0, s_pendulumLength))
				using (transform.RotateDegrees(angle))
				using (transform.Translate(0, -s_pendulumLength))
				using (transform.RotateDegrees(-direction * s_flameBlownAngle))
				using (transform.Scale(s_scale * direction, s_scale))
				using (transform.Translate(-240, -260))
					target.DrawPicture(s_tealightFlame.Picture);
			}
		}
	}
}
