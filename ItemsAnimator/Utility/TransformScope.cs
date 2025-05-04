using System;
using System.Drawing;
using SkiaSharp;

namespace ItemsAnimator.Utility;

public class TransformScope : IDisposable
{
	SKCanvas _canvas;
	SKMatrix44 _savedTransform;

	public TransformScope(SKCanvas canvas)
	{
		_canvas = canvas;
		_savedTransform = canvas.TotalMatrix44;
	}

	public static TransformScope Translate(SKCanvas canvas, float dx, float dy)
	{
		var ret = new TransformScope(canvas);

		canvas.Translate(dx, dy);

		return ret;
	}

	public static TransformScope Translate(SKCanvas canvas, SKPoint point) => Translate(canvas, point.X, point.Y);

	public TransformScope Translate(float dx, float dy)
	{
		var ret = new TransformScope(_canvas);

		_canvas.Translate(dx, dy);

		return ret;
	}

	public TransformScope Translate(SKPoint point) => Translate(point.X, point.Y);

	public static TransformScope Scale(SKCanvas canvas, float sx, float sy)
	{
		var ret = new TransformScope(canvas);

		canvas.Scale(sx, sy);

		return ret;
	}

	public TransformScope Scale(float sx, float sy)
	{
		var ret = new TransformScope(_canvas);

		_canvas.Scale(sx, sy);

		return ret;
	}

	public static TransformScope RotateDegrees(SKCanvas canvas, float degrees)
	{
		var ret = new TransformScope(canvas);

		canvas.RotateDegrees(degrees);

		return ret;
	}

	public TransformScope RotateDegrees(float degrees)
	{
		var ret = new TransformScope(_canvas);

		_canvas.RotateDegrees(degrees);

		return ret;
	}

	public static TransformScope RotateRadians(SKCanvas canvas, float radians)
	{
		var ret = new TransformScope(canvas);

		canvas.RotateRadians(radians);

		return ret;
	}

	public TransformScope RotateRadians(float radians)
	{
		var ret = new TransformScope(_canvas);

		_canvas.RotateRadians(radians);

		return ret;
	}

	public void Dispose()
	{
		_canvas.SetMatrix(_savedTransform);
	}
}
