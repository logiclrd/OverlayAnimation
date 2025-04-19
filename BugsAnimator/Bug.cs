using System;

using SkiaSharp;

namespace BugsAnimator;

class Bug
{
	public SKPoint Position;
	public SKPoint LeftEnd, RightEnd;
	public SKPoint LeftDelta, RightDelta;
	public SKPoint Jitter;
	public SKPoint Momentum;
	public float Angle;

	public void UpdateEnds(float size)
	{
		float dx = (float)(Math.Cos(Angle) * size * 0.5);
		float dy = (float)(Math.Sin(Angle) * size * 0.5);

		var d = new SKPoint(dx, dy);

		LeftEnd = PointMath.Add(Position, d);
		RightEnd = PointMath.Subtract(Position, d);
	}
}
