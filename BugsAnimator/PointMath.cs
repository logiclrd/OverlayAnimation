using System;
using System.Drawing;

using SkiaSharp;

namespace BugsAnimator;

public class PointMath
{
	public static SKPoint Add(SKPoint a, SKPoint b)
		=> new SKPoint(a.X + b.X, a.Y + b.Y);

	public static SKPoint Subtract(SKPoint a, SKPoint b)
		=> new SKPoint(a.X - b.X, a.Y - b.Y);

	public static SKPoint Multiply(SKPoint a, float multiplier)
		=> new SKPoint(a.X * multiplier, a.Y * multiplier);

	public static SKPoint Midpoint(SKPoint a, SKPoint b)
		=> new SKPoint((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);

	public static float Angle(SKPoint a, SKPoint b)
		=> (float)Math.Atan2(b.Y - a.Y, b.X - a.X);

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

	public static SKPoint RandomPoint(Random rnd, double extentX, double extentY)
		=> new SKPoint((float)(rnd.NextDouble() * extentX), (float)(rnd.NextDouble() * extentY));

	public static SKPoint ClosestPointOnLineSegment(SKPoint subject, SKPoint lineStart, SKPoint lineEnd)
	{
		var closestPointOnLine = ClosestPointOnLine(subject, lineStart, lineEnd);

		float dx = lineEnd.X - lineStart.X;
		float dy = lineEnd.Y - lineStart.Y;

		if (Math.Abs(dx) < 0.0001)
		{
			if (lineStart.Y > lineEnd.Y)
			{
				var swap = lineStart;
				lineStart = lineEnd;
				lineEnd = swap;
			}

			if (closestPointOnLine.Y < lineStart.Y)
				return lineStart;
			if (closestPointOnLine.Y > lineEnd.Y)
				return lineEnd;
		}
		else
		{
			if (lineStart.X > lineEnd.X)
			{
				var swap = lineStart;
				lineStart = lineEnd;
				lineEnd = swap;
			}

			if (closestPointOnLine.X < lineStart.X)
				return lineStart;
			if (closestPointOnLine.X > lineEnd.X)
				return lineEnd;
		}

		return closestPointOnLine;
	}

	public static SKPoint ClosestPointOnLine(SKPoint subject, SKPoint linePoint1, SKPoint linePoint2)
	{
		float dx = linePoint2.X - linePoint1.X;
		float dy = linePoint2.Y - linePoint1.Y;

		if (Math.Abs(dx) < 0.0001)
		{
			float x = (linePoint1.X + linePoint2.X) * 0.5f;

			float lineTop = Math.Min(linePoint1.Y, linePoint2.Y);
			float lineBottom = Math.Max(linePoint1.Y, linePoint2.Y);

			if (subject.Y < lineTop)
				return new SKPoint(x, lineTop);
			if (subject.Y > lineBottom)
				return new SKPoint(x, lineBottom);

			return new SKPoint(x, subject.Y);
		}
		else
		{
			float lineSlope = dy / dx;
			float normalSlope = -1.0f / lineSlope;

			// Line 1: (x - x1) / (y - y1) = (x2 - x1) / (y2 - y1)
			//         (x - x1)(y2 - y1) = (y - y1)(x2 - x1)
			//
			// Line 2: y = normalSlope * x + C
			//         C = y - normalSlope * x
			//         (y - C) = normalSlope * x
			//         x = (y - C) / normalSlope
			//         x = (C - y) * lineSlope

			float c = subject.Y - normalSlope * subject.X;

			// Inserting: (x - x1)(y2 - y1) = (y - y1)(x2 - x1)
			//
			//  Solve X   (x - x1) * lineSlope = ((normalSlope * x + C) - y1)
			//            (x - x1) * lineSlope - ((normalSlope * x + C) - y1) = 0
			//            x * lineSlope - x1 * lineSlope - ((normalSlope * x + C) - y1) = 0
			//            x * lineSlope - ((normalSlope * x + C) - y1) = x1 * lineSlope
			//            x * lineSlope - (normalSlope * x + C - y1) = x1 * lineSlope
			//            x * lineSlope - normalSlope * x - C + y1 = x1 * lineSlope
			//            x * lineSlope - normalSlope * x = x1 * lineSlope + C - y1
			//            x * (lineSlope - normalSlope) = x1 * lineSlope + C - y1
			//            x = (x1 * lineSlope + C - y1) / (lineSlope - normalSlope)
			//
			//  Solve Y   (y - y1)(x2 - x1) = (x - x1)(y2 - y1)
			//            (y1 - y) * normalSlope = ((C - y) * lineSlope - x1)
			//            (y1 - y) * normalSlope - ((C - y) * lineSlope - x1) = 0
			//            ((C - y) * lineSlope - x1) - (y1 - y) * normalSlope = 0
			//            ((C - y) * lineSlope - x1) - y1 * normalSlope + y * normalSlope = 0
			//            ((C - y) * lineSlope - x1) + y * normalSlope = y1 * normalSlope
			//            (C - y) * lineSlope - x1 + y * normalSlope = y1 * normalSlope
			//            C * lineSlope - y * lineSlope - x1 + y * normalSlope = y1 * normalSlope
			//            C * lineSlope - x1 + y * normalSlope - y * lineSlope = y1 * normalSlope
			//            y * normalSlope - y * lineSlope = y1 * normalSlope - C * lineSlope + x1
			//            y * (normalSlope - lineSlope) = y1 * normalSlope - C * lineSlope + x1
			//            y = (y1 * normalSlope - C * lineSlope + x1) / (normalSlope - lineSlope)
			//
			// This totally batshit insane crazy shit seems to actually work.

			return new SKPoint(
				x: (linePoint1.X * lineSlope + c - linePoint1.Y) / (lineSlope - normalSlope),
				y: (linePoint1.Y * normalSlope - c * lineSlope + linePoint1.X) / (normalSlope - lineSlope));
		}
	}
}