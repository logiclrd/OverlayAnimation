using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SkiaSharp;

namespace Combiner;

using SwoopAnimator;
using BugsAnimator;
using CountdownAnimator;

class Program
{
	static void Main()
	{
		var swoop = new Swoop();
		var bugs = new Bugs();
		var countdown = new Countdown();

		var swoopParameters = swoop.CreateDefaultParameters();
		var bugsParameters = bugs.CreateDefaultParameters();
		var countdownParameters = countdown.CreateDefaultParameters();

		var fonts = countdown.BuildFontList(countdownParameters);

		var swoopGenerator = swoop.RenderSequence(swoopParameters);
		var bugsGenerator = bugs.RenderSequence(bugsParameters);
		var countdownGenerator = countdown.RenderSequence(countdownParameters, fonts);

		int frameNumber = 0;

		foreach (var frame in RenderSequence(swoopGenerator, bugsGenerator, countdownGenerator))
		{
			Console.WriteLine("OUTPUT FRAME: {0}", frameNumber);

			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);

			var alphaImage = ExtractAlpha(frame);

			var alphaData = alphaImage.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("alpha" + frameNumber.ToString("d4") + ".png"))
				alphaData.SaveTo(stream);

			frameNumber++;
		}
	}

	static SKBitmap ExtractAlpha(SKBitmap bmp)
	{
		var info = new SKImageInfo(bmp.Width, bmp.Height);

		using (var surface = SKSurface.Create(info))
		{
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Black);

			var image = surface.Snapshot();

			var alpha = SKBitmap.FromImage(image);

			var bits = bmp.Bytes;

			for (int y = 0; y < bmp.Height; y++)
				for (int x = 0; x < bmp.Width; x++)
				{
					var pixel = bmp.GetPixel(x, y);

					alpha.SetPixel(x, y, new SKColor(pixel.Alpha, pixel.Alpha, pixel.Alpha));
				}

			return alpha;
		}
	}

	static IEnumerable<SKBitmap> RenderSequence(IEnumerable<SKBitmap> swoopGenerator, IEnumerable<SKBitmap> bugsGenerator, IEnumerable<SKBitmap> countdownGenerator)
	{
		// Animation sequence
		// * Take all swoop frames
		// * Fade bugs in with countdown overlayed starting 5 frames from the end
		// * Hold on last swoop frame until countdown has exhausted frames
		// * Fade everything out over 0.5s

		const int OverlapFrameCount = 5;
		const int ReminderFadeInFrames = 8;
		const int ReminderFadeOutFrames = 20;

		var swoopFrames = swoopGenerator.ToList();

		var reminderFrames = CombineFrames(bugsGenerator, countdownGenerator).ToList();

		for (int i=0; i < swoopFrames.Count - OverlapFrameCount; i++)
			yield return swoopFrames[i];

		for (int i=0; i < reminderFrames.Count; i++)
		{
			int swoopFrameIndex = swoopFrames.Count - OverlapFrameCount + i;

			if (swoopFrameIndex >= swoopFrames.Count)
				swoopFrameIndex = swoopFrames.Count - 1;

			var swoopFrame = swoopFrames[swoopFrameIndex];
			var reminderFrame = reminderFrames[i];

			var info = new SKImageInfo(swoopFrame.Width, swoopFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					swoopFrame,
					new SKPoint(0, 0),
					new SKPaint());

				float opacity;

				if (i < ReminderFadeInFrames)
					opacity = i / (float)ReminderFadeInFrames;
				else
					opacity = 1.0f;

				byte opacityByte;

				if (opacity < 0)
					opacityByte = 0;
				else if (opacity > 1.0f)
					opacityByte = 255;
				else
					opacityByte = (byte)(255 * opacity);

				canvas.DrawBitmap(
					reminderFrame,
					new SKPoint(swoopFrame.Width - reminderFrame.Width * 1.5f, (swoopFrame.Height - reminderFrame.Height) / 2),
					new SKPaint()
					{
						Color = SKColors.White.WithAlpha(opacityByte)
					});

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}

		var finalSwoopFrame = swoopFrames.Last();
		var finalReminderFrame = reminderFrames.Last();

		for (int i=0; i < ReminderFadeOutFrames; i++)
		{
			byte opacity = (byte)(255 - i * 255 / ReminderFadeOutFrames);

			var opacityPaint =
				new SKPaint()
				{
					Color = SKColors.White.WithAlpha(opacity)
				};

			var info = new SKImageInfo(finalSwoopFrame.Width, finalSwoopFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					finalSwoopFrame,
					new SKPoint(0, 0),
					opacityPaint);

				canvas.DrawBitmap(
					finalReminderFrame,
					new SKPoint(finalSwoopFrame.Width - finalReminderFrame.Width * 1.5f, (finalSwoopFrame.Height - finalReminderFrame.Height) / 2),
					opacityPaint);

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}

	static IEnumerable<SKBitmap> CombineFrames(IEnumerable<SKBitmap> bugsGenerator, IEnumerable<SKBitmap> countdownGenerator)
	{
		var bugsFrames = bugsGenerator.GetEnumerator();
		var countdownFrames = countdownGenerator.GetEnumerator();

		while (bugsFrames.MoveNext() && countdownFrames.MoveNext())
		{
			var bugsFrame = bugsFrames.Current;
			var countdownFrame = countdownFrames.Current;

			var info = new SKImageInfo(bugsFrame.Width, bugsFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					bugsFrame,
					new SKPoint(0, 0),
					new SKPaint());

				canvas.DrawBitmap(
					countdownFrame,
					new SKPoint((bugsFrame.Width - countdownFrame.Width) / 2, (bugsFrame.Height - countdownFrame.Height) / 2),
					new SKPaint());

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}
}
