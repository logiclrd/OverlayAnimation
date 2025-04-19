using System;
using System.Collections.Generic;
using System.Linq;

using SkiaSharp;

namespace SwoopAnimator;

public class Swoop
{
	public Parameters CreateDefaultParameters()
	{
		return
			new Parameters()
			{
				RenderWidth = 1920,
				RenderHeight = 350,
				LeadInAreaWidth = 100,
				SwoopStrokeExtentMin = 250,
				SwoopStrokeExtentMax = 400,
				SwoopStrokeAdvanceMin = 150,
				SwoopStrokeAdvanceMax = 300,
				SwoopColour = new SKColor(0xff04c4c7),
				BackgroundColour = SKColors.White, //SKColors.Transparent,
				SwoopStrokeBrushWidth = 120,
				SwoopStrokeBrushAngle = 15 / 57.295779f,
				SwoopStrokeBrushAngleDelta = 10 / 57.295779f,
				SwoopStrokeBend = 0.05f,
				SwoopStrokeBrushDotSize = 4.5f,
				SwoopStrokeBrushDotSizeVariance = 1f,
				AnimationDuration = 2.5f,
				FramesPerSecond = 30000f / 1001f,
			};
	}

	SKPoint[] CreateMainSwoop(Parameters parameters, Random rnd)
	{
		var points = new List<SKPoint>();

		// Start with a high point.
		float xHigh = parameters.LeadInAreaWidth * (float)(-rnd.NextDouble() - 0.5);
		float yHigh = parameters.RenderHeight * (float)rnd.NextDouble() * 0.3f;

		points.Add(new SKPoint(xHigh, yHigh));

		float xLow = xHigh + 1;
		float yLow = yHigh + 10;

		Console.WriteLine("High point: {0}", points[0]);

		while (true)
		{
			// Next low point.
			float nextYLow = parameters.RenderHeight * (float)(rnd.NextDouble() * 0.3 + 0.7);

			Console.WriteLine("Plan next low Y: {0}", nextYLow);

			float minimumNextXLow;

			if (xHigh >= xLow)
			{
				Console.WriteLine("Return line has forward slope, no restriction needed");

				minimumNextXLow = xLow + parameters.SwoopStrokeExtentMin;
			}
			else
			{
				float lastStrokeSlope = (yHigh - yLow) / (xLow - xHigh);

				Console.WriteLine("Return line has slope: {0}", lastStrokeSlope);

				float lastStrokeIntersectionWithNextYLow = xHigh + (nextYLow - yHigh) / lastStrokeSlope;

				Console.WriteLine("Extrapolating, the X coordinate where the return line intersects the planned Y is: {0}", lastStrokeIntersectionWithNextYLow);

				minimumNextXLow = lastStrokeIntersectionWithNextYLow + 10;
			}

			if (minimumNextXLow < xLow + parameters.SwoopStrokeExtentMin)
				minimumNextXLow = xLow + parameters.SwoopStrokeExtentMin;

			float maximumNextXLow = xHigh + parameters.SwoopStrokeExtentMax;

			if (maximumNextXLow < minimumNextXLow)
				maximumNextXLow = minimumNextXLow;

			Console.WriteLine("Next low X range: [{0}, {1}]", minimumNextXLow, maximumNextXLow);

			xLow = (float)(rnd.NextDouble() * (maximumNextXLow - minimumNextXLow) + minimumNextXLow);
			yLow = nextYLow;

			if (xLow > parameters.RenderWidth)
			{
				Console.WriteLine("This swoop would exit the frame, exiting loop");
				points.RemoveAt(points.Count - 1);
				break;
			}

			points.Add(new SKPoint(xLow, yLow));

			Console.WriteLine("Low point: {0}", points.Last());

			// Next high point.
			/*
			float maximumNextXHigh = xHigh + parameters.SwoopStrokeAdvanceMax;

			if (maximumNextXHigh > xLow - 10)
				maximumNextXHigh = xLow - 10;

			float minimumNextXHigh = xHigh + parameters.SwoopStrokeAdvanceMin;
			*/
			float maximumNextXHigh = xLow - parameters.SwoopStrokeAdvanceMin;
			float minimumNextXHigh = xLow - parameters.SwoopStrokeAdvanceMax;

			if (minimumNextXHigh > maximumNextXHigh)
				minimumNextXHigh = maximumNextXHigh;

			xHigh = (float)(rnd.NextDouble() * (maximumNextXHigh - minimumNextXHigh) + minimumNextXHigh);
			yHigh = parameters.RenderHeight * (float)rnd.NextDouble() * 0.3f;

			points.Add(new SKPoint(xHigh, yHigh));

			Console.WriteLine("High point: {0}", points.Last());
		}

		return points.ToArray();
	}

	SKBitmap InitializeFrame(Parameters parameters)
	{
		var info = new SKImageInfo(parameters.RenderWidth, parameters.RenderHeight);

		using (var surface = SKSurface.Create(info))
		{
			var canvas = surface.Canvas;

			canvas.Clear(parameters.BackgroundColour);

			var image = surface.Snapshot();

			return SKBitmap.FromImage(image);
		}
	}

	void ProduceSwoop(Parameters parameters, SKPoint[] mainSwoopPoints, SKPoint brushVector, Action<double, SKPoint, SKPoint> callback)
	{
		double overallT = 0.0;

		for (int i = 1; i < mainSwoopPoints.Length; i++)
		{
			SKPoint from = mainSwoopPoints[i - 1];
			SKPoint to = mainSwoopPoints[i];

			var midpoint = PointMath.Midpoint(from, to);
			var length = PointMath.Length(from, to);
			var slope = PointMath.Slope(from, to);

			var perpendicularSlope = -1.0f / slope;

			SKPoint normal = new SKPoint(1.0f, perpendicularSlope);

			normal = PointMath.Scale(normal, 1.0f / PointMath.Magnitude(normal));

			SKPoint bendVector = PointMath.Scale(normal, length * parameters.SwoopStrokeBend);

			for (float dt = parameters.SwoopStrokeBrushDotSize * 0.25f / length, t = 0.0f; t < 1.0f; t += dt)
			{
				var brushAngle = parameters.SwoopStrokeBrushAngle + parameters.SwoopStrokeBrushAngleDelta * (0.5f - t);

				SKPoint straightLinePoint = PointMath.Interpolate(from, to, t);

				float bendFactor = (float)(1 - Math.Pow(2 * (t - 0.5), 2));

				var bentPoint = PointMath.Add(straightLinePoint, PointMath.Scale(bendVector, bendFactor));

				var rotatedBrushVector = PointMath.Rotate(brushVector, -brushAngle);
				
				callback(overallT + t * length, bentPoint, rotatedBrushVector);
			}
		}
	}

	double MeasureSwoop(Parameters parameters, SKPoint[] mainSwoopPoints)
	{
		double length = 0.0;

		SKPoint lastPoint = mainSwoopPoints[0];

		ProduceSwoop(
			parameters,
			mainSwoopPoints,
			brushVector: new SKPoint(parameters.SwoopStrokeBrushWidth * 0.5f, 0),
			(t, bentPoint, rotatedBrushVector) =>
			{
				length += PointMath.Length(lastPoint, bentPoint);

				lastPoint = bentPoint;
			});

		return length;
	}

	void RenderSwoop(Parameters parameters, Random rnd, SKPoint[] mainSwoopPoints, double fromT, double toT, SKBitmap target)
	{
		float[] brushNoise = new float[parameters.SwoopStrokeBrushWidth];

		for (int i=0; i < brushNoise.Length; i++)
		{
			float t = Math.Abs(i - 90f) / 90f;

			t = t - (float)rnd.NextDouble() * 0.8f;

			t -= 0.15f;

			brushNoise[i] = t;
		}

		int minD = (int)(-parameters.SwoopStrokeBrushDotSize * 0.35f);
		int maxD = minD + (int)Math.Round(parameters.SwoopStrokeBrushDotSize * 0.7f);

		double overallT = 0.0;

		SKPoint lastPoint = mainSwoopPoints[0];

		ProduceSwoop(
			parameters,
			mainSwoopPoints,
			brushVector: new SKPoint(parameters.SwoopStrokeBrushWidth * 0.5f, 0),
			(t, bentPoint, rotatedBrushVector) =>
			{
				double thisPointOverallT = overallT;

				overallT += PointMath.Length(lastPoint, bentPoint);
				
				lastPoint = bentPoint;

				var brushStart = PointMath.Subtract(bentPoint, rotatedBrushVector);
				var brushEnd = PointMath.Add(bentPoint, rotatedBrushVector);

				for (float brushI = 0.0f;
					brushI < parameters.SwoopStrokeBrushWidth;
					brushI += PointMath.Interpolate(parameters.SwoopStrokeBrushDotSize - parameters.SwoopStrokeBrushDotSizeVariance * 0.5f, parameters.SwoopStrokeBrushDotSize + parameters.SwoopStrokeBrushDotSizeVariance * 0.5f, (float)rnd.NextDouble()))
				{
					if (brushI >= brushNoise.Length)
						brushI = brushNoise.Length - 1;

					float brushT = brushI / parameters.SwoopStrokeBrushWidth;
					int brushIndex = (int)Math.Round(brushI);

					if (brushIndex < 0)
						brushIndex = 0;
					if (brushIndex >= brushNoise.Length)
						brushIndex = brushNoise.Length - 1;

					float noiseFactor = brushNoise[brushIndex];

					if (noiseFactor < 0.0f)
						noiseFactor = 0.0f;

					noiseFactor += 0.2f / (brushT + 1);
					noiseFactor += 0.2f / (2 - brushT);

					if (rnd.NextDouble() > noiseFactor)
					{
						if ((thisPointOverallT >= fromT) && (thisPointOverallT <= toT))
						{
							var brushPoint = PointMath.Interpolate(brushStart, brushEnd, brushT);

							var pixelPoint = PointMath.ToInts(brushPoint);

							int minDX = minD, minDY = minD;
							int maxDX = maxD, maxDY = maxD;

							if (pixelPoint.X + minDX < 0)
								minDX = -pixelPoint.X;
							if (pixelPoint.Y + minDY < 0)
								minDY = -pixelPoint.Y;
							if (pixelPoint.X + maxDX > parameters.RenderWidth)
								maxDX = parameters.RenderWidth - pixelPoint.X;
							if (pixelPoint.Y + maxDY > parameters.RenderHeight)
								maxDY = parameters.RenderHeight - pixelPoint.Y;

							for (int dx = minDX; dx < maxDX; dx++)
								for (int dy = minDY; dy < maxDY; dy++)
									target.SetPixel(pixelPoint.X + dx, pixelPoint.Y + dy, parameters.SwoopColour);
						}
					}
				}

				for (int j=0; j < brushNoise.Length; j++)
					brushNoise[j] += ((float)rnd.NextDouble() - 0.5f) * 0.1f;
			});
	}

	public IEnumerable<SKBitmap> RenderSequence(Parameters parameters)
	{
		var rnd = new Random();

		var mainSwoopPoints = CreateMainSwoop(parameters, rnd);

		double maxT = MeasureSwoop(parameters, mainSwoopPoints);

		int frameCount = (int)Math.Round(parameters.AnimationDuration * parameters.FramesPerSecond);

		for (int frameNumber = 0; frameNumber < frameCount; frameNumber++)
		{
			double frameT = (frameCount - frameNumber) / (double)frameCount;

			double startT = Math.Max(0, maxT * (0.8 - 2 * Math.Pow(frameT, 2)));
			double endT = maxT * (1 - Math.Pow(frameT, 2));

			Console.WriteLine("{0} / {1}: {2} to {3} (out of {4})", frameNumber, frameCount, startT, endT, maxT);

			var frame = InitializeFrame(parameters);

			RenderSwoop(
				parameters,
				new Random(parameters.RandomSeed),
				mainSwoopPoints,
				startT,
				endT,
				frame);

			yield return frame;
		}
	}
}
