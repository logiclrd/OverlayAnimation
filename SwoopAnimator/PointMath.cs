using System;
using System.Drawing;

using SkiaSharp;

namespace SwoopAnimator;

public class PointMath
{
	public static SKPoint Add(SKPoint a, SKPoint b)
		=> new SKPoint(a.X + b.X, a.Y + b.Y);

	public static SKPoint Subtract(SKPoint a, SKPoint b)
		=> new SKPoint(a.X - b.X, a.Y - b.Y);

	public static SKPoint Midpoint(SKPoint a, SKPoint b)
		=> new SKPoint((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);

	public static SKPoint Rotate(SKPoint v, float angle)
	{
		var cos = (float)Math.Cos(angle);
		var sin = (float)Math.Sin(angle);

		return new SKPoint(cos * v.X - sin * v.Y, sin * v.X + cos * v.Y);
	}

	public static float Magnitude(SKPoint v)
		=> (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);

	public static float Length(SKPoint a, SKPoint b)
	{
		float dx = a.X - b.X;
		float dy = a.Y - b.Y;

		return (float)Math.Sqrt(dx * dx + dy * dy);
	}

	public static float Slope(SKPoint a, SKPoint b)
		=> (b.Y - a.Y) / (b.X - a.X);

	public static SKPoint Scale(SKPoint a, float factor)
		=> new SKPoint(a.X * factor, a.Y * factor);

	public static float Interpolate(float a, float b, float t)
		=> a + (b - a) * t;

	public static SKPoint Interpolate(SKPoint a, SKPoint b, float t)
		=> new SKPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

	public static Point ToInts(SKPoint a)
		=> new Point((int)Math.Round(a.X), (int)Math.Round(a.Y));

	public static bool IsContained(Point pt, Rectangle r)
	{
		if (pt.X < r.Left)
			return false;
		if (pt.X >= r.Right)
			return false;
		if (pt.Y < r.Top)
			return false;
		if (pt.Y >= r.Bottom)
			return false;

		return true;
	}
}