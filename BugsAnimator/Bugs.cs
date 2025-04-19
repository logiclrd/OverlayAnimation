using System;
using System.Collections.Generic;
using System.Linq;

using SkiaSharp;

namespace BugsAnimator;

public class Bugs
{
	public void Advance(Random rnd, Bug[] bugs, Parameters parameters)
	{
		for (int i=0; i < bugs.Length; i++)
		{
			bugs[i].Momentum = PointMath.Multiply(bugs[i].Momentum, 0.9f);
			bugs[i].Momentum = PointMath.Add(bugs[i].Momentum, PointMath.Multiply(bugs[i].Jitter, 0.1f));

			bugs[i].Jitter = PointMath.Multiply(bugs[i].Jitter, 0.9f);
			bugs[i].Jitter = PointMath.Add(
				bugs[i].Jitter,
				PointMath.Rotate(
					new SKPoint((float)(rnd.NextDouble() * parameters.Jitter), 0),
					(float)(rnd.NextDouble() * Math.PI * 2)));

			bugs[i].Position = PointMath.Add(bugs[i].Position, bugs[i].Momentum);

			bugs[i].UpdateEnds(parameters.BugSize);
		}

		for (int i=0; i < bugs.Length; i++)
		{
			bugs[i].LeftDelta = new SKPoint(0, 0);
			bugs[i].RightDelta = new SKPoint(0, 0);

			for (int j=0; j < bugs.Length; j++)
			{
				if (i == j)
					continue;

				void Repel(ref SKPoint delta, ref SKPoint repulsee)
				{
					float distance = PointMath.Length(bugs[i].Position, bugs[j].Position);

					if (distance > parameters.MaximumDistance)
						return;

					if (distance < parameters.BugSize * 1.5f)
					{
						var vector = PointMath.Subtract(bugs[i].Position, bugs[j].Position);

						vector = PointMath.Multiply(vector, 5 * parameters.RepulsionStrength / (distance + 1));

						delta = PointMath.Add(bugs[i].LeftDelta, vector);
					}
					else
					{
						var nearest = PointMath.ClosestPointOnLineSegment(bugs[i].LeftEnd, bugs[j].LeftEnd, bugs[j].RightEnd);

						var vector = PointMath.Subtract(repulsee, nearest);

						float magnitude = PointMath.Magnitude(vector);

						vector = PointMath.Multiply(vector, parameters.RepulsionStrength / magnitude);

						delta = PointMath.Add(bugs[i].LeftDelta, vector);
					}
				}

				Repel(ref bugs[i].LeftDelta, ref bugs[i].LeftEnd);
				Repel(ref bugs[i].RightDelta, ref bugs[i].RightEnd);
			}
		}

		for (int i=0; i < bugs.Length; i++)
		{
			if (!float.IsNormal(bugs[i].LeftDelta.X))
				bugs[i].LeftDelta.X = 0;
			if (!float.IsNormal(bugs[i].LeftDelta.Y))
				bugs[i].LeftDelta.Y = 0;

			if (!float.IsNormal(bugs[i].RightDelta.X))
				bugs[i].RightDelta.X = 0;
			if (!float.IsNormal(bugs[i].RightDelta.Y))
				bugs[i].RightDelta.Y = 0;

			bugs[i].LeftEnd = PointMath.Add(bugs[i].LeftEnd, bugs[i].LeftDelta);
			bugs[i].RightEnd = PointMath.Add(bugs[i].RightEnd, bugs[i].RightDelta);

			bugs[i].Position = PointMath.Midpoint(bugs[i].LeftEnd, bugs[i].RightEnd);
			bugs[i].Angle = PointMath.Angle(bugs[i].LeftEnd, bugs[i].RightEnd);

			if (bugs[i].Position.X < 0)
				bugs[i].Position.X = 0;
			if (bugs[i].Position.Y < 0)
				bugs[i].Position.Y = 0;
			if (bugs[i].Position.X > parameters.RenderWidth)
				bugs[i].Position.X = parameters.RenderWidth;
			if (bugs[i].Position.Y > parameters.RenderHeight)
				bugs[i].Position.Y = parameters.RenderHeight;

			bugs[i].UpdateEnds(parameters.BugSize);
		}
	}

	public SKBitmap Render(Bug[] bugs, Parameters parameters)
	{
		var info = new SKImageInfo(parameters.RenderWidth, parameters.RenderHeight);

		using (var surface = SKSurface.Create(info))
		{
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			var drawingArea = new SKRoundRect();

			float halfPen = parameters.BorderThickness * 0.5f;
			var cornerRadius = new SKPoint(parameters.CornerRadius, parameters.CornerRadius);

			drawingArea.SetRectRadii(
				new SKRect(halfPen + 1, halfPen + 1, parameters.RenderWidth - halfPen - 2, parameters.RenderHeight - halfPen - 2),
				new SKPoint[]
				{
					cornerRadius,
					cornerRadius,
					cornerRadius,
					cornerRadius,
				});

			canvas.DrawRoundRect(
				drawingArea,
				new SKPaint()
				{
					Style = SKPaintStyle.Fill,
					Color = SKColors.White,
				});

			canvas.ClipRoundRect(drawingArea, SKClipOperation.Intersect);

			var bugPaint = new SKPaint();

			bugPaint.Color = SKColors.LightGray;
			bugPaint.StrokeCap = SKStrokeCap.Round;
			bugPaint.StrokeWidth = parameters.BugThickness;
			bugPaint.Style = SKPaintStyle.Stroke;
			bugPaint.IsAntialias = true;

			for (int i=0; i < bugs.Length; i++)
				canvas.DrawLine(bugs[i].LeftEnd, bugs[i].RightEnd, bugPaint);

			canvas.DrawRoundRect(
				drawingArea,
				new SKPaint()
				{
					Style = SKPaintStyle.Stroke,
					Color = SKColors.Black,
					StrokeWidth = parameters.BorderThickness,
					IsAntialias = true,
				});

			var image = surface.Snapshot();

			return SKBitmap.FromImage(image);
		}
	}

	public Parameters CreateDefaultParameters()
	{
		return
			new Parameters()
			{
				FrameCount = 500,
				RenderWidth = 400,
				RenderHeight = 320,
				CornerRadius = 30,
				BorderThickness = 3,
				BugCount = 1000,
				BugSize = 6,
				BugThickness = 3,
				RepulsionStrength = 0.1f,
				MaximumDistance = 15,
				Jitter = 0.25f,
			};
	}

	public IEnumerable<SKBitmap> RenderSequence(Parameters parameters)
	{
		Random rnd = new Random();

		Bug[] bugs = new Bug[parameters.BugCount];

		for (int i=0; i < bugs.Length; i++)
		{
			if ((i % 100) == 99)
				Console.Write('#');
			else
				Console.Write('.');

			do
				bugs[i] =
					new Bug()
					{
						Position = PointMath.RandomPoint(rnd, parameters.RenderWidth, parameters.RenderHeight),
						Angle = (float)(rnd.Next() * 2 * Math.PI),
					};
			while (bugs.Take(i - 1).Any(otherBug => PointMath.Length(bugs[i].Position, otherBug.Position) < 9));
		}

		Console.WriteLine();

		for (int frameNumber=0; frameNumber < parameters.FrameCount; frameNumber++)
		{
			Console.WriteLine(frameNumber);

			Advance(rnd, bugs, parameters);

			yield return Render(bugs, parameters);
		}
	}
}
