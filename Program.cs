using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SkiaSharp;

namespace SwoopAnimator;

class Parameters
{
	public int RandomSeed;
	public int RenderWidth = 1920;
	public int RenderHeight = 350;
	public int LeadInAreaWidth = 100;
	public float SwoopStrokeExtentMin = 250;
	public float SwoopStrokeExtentMax = 400;
	public float SwoopStrokeAdvanceMin = 150;
	public float SwoopStrokeAdvanceMax = 300;
	public SKColor BackgroundColour = SKColors.White; //SKColors.Transparent;
	public int SwoopStrokeBrushWidth = 120;
	public float SwoopStrokeBrushAngle = 15 / 57.295779f;
	public float SwoopStrokeBrushAngleDelta = 10 / 57.295779f;
	public float SwoopStrokeBend = 0.05f;
	public float SwoopStrokeBrushDotSize = 4.5f;
	public float SwoopStrokeBrushDotSizeVariance = 1f;
	public float AnimationDuration = 2.5f;
	public float FramesPerSecond = 30000f / 1001f;
}

class Program
{
	static Parameters s_params = new Parameters();

	static Random CreateRandomSource()
	{
		return new Random(Seed: s_params.RandomSeed);
	}

	static SKPoint[] CreateMainSwoop(Random rnd)
	{
		var points = new List<SKPoint>();

		// Start with a high point.
		float xHigh = s_params.LeadInAreaWidth * (float)(-rnd.NextDouble() - 0.5);
		float yHigh = s_params.RenderHeight * (float)rnd.NextDouble() * 0.3f;

		points.Add(new SKPoint(xHigh, yHigh));

		float xLow = xHigh + 1;
		float yLow = yHigh + 10;

		Console.WriteLine("High point: {0}", points[0]);

		while (true)
		{
			// Next low point.
			float nextYLow = s_params.RenderHeight * (float)(rnd.NextDouble() * 0.3 + 0.7);

			Console.WriteLine("Plan next low Y: {0}", nextYLow);

			float minimumNextXLow;

			if (xHigh >= xLow)
			{
				Console.WriteLine("Return line has forward slope, no restriction needed");

				minimumNextXLow = xLow + s_params.SwoopStrokeExtentMin;
			}
			else
			{
				float lastStrokeSlope = (yHigh - yLow) / (xLow - xHigh);

				Console.WriteLine("Return line has slope: {0}", lastStrokeSlope);

				float lastStrokeIntersectionWithNextYLow = xHigh + (nextYLow - yHigh) / lastStrokeSlope;

				Console.WriteLine("Extrapolating, the X coordinate where the return line intersects the planned Y is: {0}", lastStrokeIntersectionWithNextYLow);

				minimumNextXLow = lastStrokeIntersectionWithNextYLow + 10;
			}

			if (minimumNextXLow < xLow + s_params.SwoopStrokeExtentMin)
				minimumNextXLow = xLow + s_params.SwoopStrokeExtentMin;

			float maximumNextXLow = xHigh + s_params.SwoopStrokeExtentMax;

			if (maximumNextXLow < minimumNextXLow)
				maximumNextXLow = minimumNextXLow;

			Console.WriteLine("Next low X range: [{0}, {1}]", minimumNextXLow, maximumNextXLow);

			xLow = (float)(rnd.NextDouble() * (maximumNextXLow - minimumNextXLow) + minimumNextXLow);
			yLow = nextYLow;

			if (xLow > s_params.RenderWidth)
			{
				Console.WriteLine("This swoop would exit the frame, exiting loop");
				points.RemoveAt(points.Count - 1);
				break;
			}

			points.Add(new SKPoint(xLow, yLow));

			Console.WriteLine("Low point: {0}", points.Last());

			// Next high point.
			/*
			float maximumNextXHigh = xHigh + s_params.SwoopStrokeAdvanceMax;

			if (maximumNextXHigh > xLow - 10)
				maximumNextXHigh = xLow - 10;

			float minimumNextXHigh = xHigh + s_params.SwoopStrokeAdvanceMin;
			*/
			float maximumNextXHigh = xLow - s_params.SwoopStrokeAdvanceMin;
			float minimumNextXHigh = xLow - s_params.SwoopStrokeAdvanceMax;

			if (minimumNextXHigh > maximumNextXHigh)
				minimumNextXHigh = maximumNextXHigh;

			xHigh = (float)(rnd.NextDouble() * (maximumNextXHigh - minimumNextXHigh) + minimumNextXHigh);
			yHigh = s_params.RenderHeight * (float)rnd.NextDouble() * 0.3f;

			points.Add(new SKPoint(xHigh, yHigh));

			Console.WriteLine("High point: {0}", points.Last());
		}

		return points.ToArray();
	}

	static SKBitmap InitializeFrame()
	{
		var info = new SKImageInfo(s_params.RenderWidth, s_params.RenderHeight);

		using (var surface = SKSurface.Create(info))
		{
			var canvas = surface.Canvas;

			canvas.Clear(s_params.BackgroundColour);

			var image = surface.Snapshot();

			return SKBitmap.FromImage(image);
		}
	}

	static void ProduceSwoop(SKPoint[] mainSwoopPoints, SKPoint brushVector, Action<double, SKPoint, SKPoint> callback)
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

			SKPoint bendVector = PointMath.Scale(normal, length * s_params.SwoopStrokeBend);

			for (float dt = s_params.SwoopStrokeBrushDotSize * 0.25f / length, t = 0.0f; t < 1.0f; t += dt)
			{
				var brushAngle = s_params.SwoopStrokeBrushAngle + s_params.SwoopStrokeBrushAngleDelta * (0.5f - t);

				SKPoint straightLinePoint = PointMath.Interpolate(from, to, t);

				float bendFactor = (float)(1 - Math.Pow(2 * (t - 0.5), 2));

				var bentPoint = PointMath.Add(straightLinePoint, PointMath.Scale(bendVector, bendFactor));

				var rotatedBrushVector = PointMath.Rotate(brushVector, -brushAngle);
				
				callback(overallT + t * length, bentPoint, rotatedBrushVector);
			}
		}
	}

	static double MeasureSwoop(SKPoint[] mainSwoopPoints)
	{
		double length = 0.0;

		SKPoint lastPoint = mainSwoopPoints[0];

		ProduceSwoop(
			mainSwoopPoints,
			brushVector: new SKPoint(s_params.SwoopStrokeBrushWidth * 0.5f, 0),
			(t, bentPoint, rotatedBrushVector) =>
			{
				length += PointMath.Length(lastPoint, bentPoint);

				lastPoint = bentPoint;
			});

		return length;
	}

	static void RenderSwoop(Random rnd, SKPoint[] mainSwoopPoints, double fromT, double toT, SKBitmap target)
	{
		float[] brushNoise = new float[s_params.SwoopStrokeBrushWidth];

		for (int i=0; i < brushNoise.Length; i++)
		{
			float t = Math.Abs(i - 90f) / 90f;

			t = t - (float)rnd.NextDouble() * 0.8f;

			t -= 0.15f;

			brushNoise[i] = t;
		}

		int minD = (int)(-s_params.SwoopStrokeBrushDotSize * 0.35f);
		int maxD = minD + (int)Math.Round(s_params.SwoopStrokeBrushDotSize * 0.7f);

		double overallT = 0.0;

		SKPoint lastPoint = mainSwoopPoints[0];

		ProduceSwoop(
			mainSwoopPoints,
			brushVector: new SKPoint(s_params.SwoopStrokeBrushWidth * 0.5f, 0),
			(t, bentPoint, rotatedBrushVector) =>
			{
				double thisPointOverallT = overallT;

				overallT += PointMath.Length(lastPoint, bentPoint);
				
				lastPoint = bentPoint;

				var brushStart = PointMath.Subtract(bentPoint, rotatedBrushVector);
				var brushEnd = PointMath.Add(bentPoint, rotatedBrushVector);

				for (float brushI = 0.0f;
					brushI < s_params.SwoopStrokeBrushWidth;
					brushI += PointMath.Interpolate(s_params.SwoopStrokeBrushDotSize - s_params.SwoopStrokeBrushDotSizeVariance * 0.5f, s_params.SwoopStrokeBrushDotSize + s_params.SwoopStrokeBrushDotSizeVariance * 0.5f, (float)rnd.NextDouble()))
				{
					if (brushI >= brushNoise.Length)
						brushI = brushNoise.Length - 1;

					float brushT = brushI / s_params.SwoopStrokeBrushWidth;
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
							if (pixelPoint.X + maxDX > s_params.RenderWidth)
								maxDX = s_params.RenderWidth - pixelPoint.X;
							if (pixelPoint.Y + maxDY > s_params.RenderHeight)
								maxDY = s_params.RenderHeight - pixelPoint.Y;

							for (int dx = minDX; dx < maxDX; dx++)
								for (int dy = minDY; dy < maxDY; dy++)
									target.SetPixel(pixelPoint.X + dx, pixelPoint.Y + dy, SKColors.Teal);
						}
					}
				}

				for (int j=0; j < brushNoise.Length; j++)
					brushNoise[j] += ((float)rnd.NextDouble() - 0.5f) * 0.1f;
			});
	}

	static void Main()
	{
		var rnd = new Random();

		s_params.RandomSeed = rnd.Next();

		var mainSwoopPoints = CreateMainSwoop(rnd);

		double maxT = MeasureSwoop(mainSwoopPoints);

		int frameCount = (int)Math.Round(s_params.AnimationDuration * s_params.FramesPerSecond);

		for (int frameNumber = 0; frameNumber < frameCount; frameNumber++)
		{
			double frameT = (frameCount - frameNumber) / (double)frameCount;

			double startT = Math.Max(0, maxT * (0.8 - 2 * Math.Pow(frameT, 2)));
			double endT = maxT * (1 - Math.Pow(frameT, 2));

			Console.WriteLine("{0} / {1}: {2} to {3} (out of {4})", frameNumber, frameCount, startT, endT, maxT);

			var frame = InitializeFrame();

			RenderSwoop(
				CreateRandomSource(),
				mainSwoopPoints,
				startT,
				endT,
				frame);

			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);
		}
	}
}
