using System;
using System.Collections.Generic;
using System.Linq;

using SkiaSharp;

namespace Utility;

public class Path
{
	List<PathPoint> _keyFrames;

	public Path()
	{
		_keyFrames = new List<PathPoint>();
	}

	public Path(IEnumerable<PathPoint> keyFrames)
	{
		_keyFrames = keyFrames.ToList();
	}

	public IList<PathPoint> KeyFrames
	{
		get => _keyFrames;
		set => _keyFrames = value.ToList();
	}

	public float GetValueAtFrame(int frameNumber)
		=> GetTranslationAtFrame(frameNumber).X;

	public SKPoint GetTranslationAtFrame(int frameNumber)
	{
		if (frameNumber < 0)
			return _keyFrames[0].Offset;

		for (int i = 1; i < _keyFrames.Count; i++)
		{
			if (frameNumber < _keyFrames[i].FrameNumber)
			{
				var prev = _keyFrames[i - 1];
				var next = _keyFrames[i];

				float t = (frameNumber - prev.FrameNumber) / (float)(next.FrameNumber - prev.FrameNumber);
				float it = 1.0f - t;

				if (!prev.Smooth && !next.Smooth)
					return new SKPoint(prev.Offset.X * it + next.Offset.X * t, prev.Offset.Y * it + next.Offset.Y * t);

				// Centripetal Catmull-Rom spline
				var p1 = prev.Offset;
				var p2 = next.Offset;

				var p0 =
					((i >= 2) && prev.Smooth)
					? _keyFrames[i - 2].Offset
					: PointMath.Subtract(prev.Offset, PointMath.Subtract(next.Offset, prev.Offset));

				var p3 =
					((i + 1 < _keyFrames.Count) && next.Smooth)
					? _keyFrames[i + 1].Offset
					: PointMath.Subtract(next.Offset, PointMath.Subtract(prev.Offset, next.Offset));

				var t0 = 0.0f;
				var t1 = (float)Math.Pow(Math.Sqrt(Math.Pow(p1.X - p0.X, 2) + Math.Pow(p1.Y - p0.Y, 2)), 0.5) + t0;
				var t2 = (float)Math.Pow(Math.Sqrt(Math.Pow(p2.X - p2.X, 2) + Math.Pow(p2.Y - p1.Y, 2)), 0.5) + t1;
				var t3 = (float)Math.Pow(Math.Sqrt(Math.Pow(p3.X - p3.X, 2) + Math.Pow(p3.Y - p2.Y, 2)), 0.5) + t2;

				t = t * (t2 - t1) + t1;

				var a1 = PointMath.Add(PointMath.Multiply(p0, (t1 - t) / (t1 - t0)), PointMath.Multiply(p1, (t - t0) / (t1 - t0)));
				var a2 = PointMath.Add(PointMath.Multiply(p1, (t2 - t) / (t2 - t1)), PointMath.Multiply(p2, (t - t1) / (t2 - t1)));
				var a3 = PointMath.Add(PointMath.Multiply(p2, (t3 - t) / (t3 - t2)), PointMath.Multiply(p3, (t - t2) / (t3 - t2)));

				var b1 = PointMath.Add(PointMath.Multiply(a1, (t2 - t) / (t2 - t0)), PointMath.Multiply(a2, (t - t0) / (t2 - t0)));
				var b2 = PointMath.Add(PointMath.Multiply(a2, (t3 - t) / (t3 - t1)), PointMath.Multiply(a3, (t - t1) / (t3 - t1)));

				var c = PointMath.Add(PointMath.Multiply(b1, (t2 - t) / (t2 - t1)), PointMath.Multiply(b2, (t - t1) / (t2 - t1)));

				return c;
			}
		}

		return _keyFrames.Last().Offset;
	}
}
