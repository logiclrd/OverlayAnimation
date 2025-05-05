using System;
using System.Collections.Generic;
using System.IO;

using SkiaSharp;

namespace CountdownAnimator;

public class Countdown
{
	public Parameters CreateDefaultParameters()
	{
		return
			new Parameters()
			{
				RenderWidth = 300,
				RenderHeight = 200,
				FontDirectory = "/home/logiclrd/90sFonts",
				FontSize = 100,
				ChangeFontAfterFrames = 24,
				ChangeColourAfterFrames = 24,
				CountdownSeconds = 15,
				DropShadowSize = 15,
				FramesPerSecond = 24, // Animation is too short to worry about NTSC denominators.
			};
	}

	public List<SKFont> BuildFontList(Parameters parameters)
	{
		List<SKFont> fonts = new List<SKFont>();

		foreach (var fileName in Directory.GetFiles(parameters.FontDirectory))
		{
			string extension = Path.GetExtension(fileName);

			if (string.Equals(extension, ".ttf", System.StringComparison.InvariantCultureIgnoreCase)
			 || string.Equals(extension, ".otf", System.StringComparison.InvariantCultureIgnoreCase))
			{
				string filePath = Path.Combine(parameters.FontDirectory, fileName);

				var typeface = SKTypeface.FromFile(filePath);

				fonts.Add(new SKFont(typeface, parameters.FontSize));
			}
		}

		return fonts;
	}

	public IEnumerable<SKBitmap> RenderSequence(Parameters parameters, IList<SKFont> fonts)
	{
		Random rnd = new Random();

		SKColor PickRandomColour() => SKColor.FromHsl(
			(float)(rnd.NextDouble() * 360),
			s: 100,
			l: 50);

		SKFont currentFont = default;

		SKFont PickRandomFont()
		{
			while (true)
			{
				var nextFont = fonts[rnd.Next(fonts.Count)];

				if (nextFont != currentFont)
					return nextFont;
			}
		}

		currentFont = PickRandomFont();

		int frameCount = parameters.CountdownSeconds * parameters.FramesPerSecond;

		var textPosition = new SKPoint(parameters.RenderWidth * 0.5f, parameters.RenderHeight * 0.75f);

		var paint = new SKPaint();

		paint.Color = PickRandomColour();

		var shadowPaint = new SKPaint();

		paint.Color = SKColors.Black;

		for (int frameNumber = 0; frameNumber < frameCount; frameNumber++)
		{
			int secondIndex = frameNumber / parameters.FramesPerSecond;

			int secondValue = parameters.CountdownSeconds - secondIndex;

			if ((frameNumber % parameters.ChangeFontAfterFrames) == 0)
				currentFont = PickRandomFont();

			if ((frameNumber % parameters.ChangeColourAfterFrames) == 0)
				paint.Color = PickRandomColour();

			Console.WriteLine("{0}: {1} {2}", frameNumber, secondValue, currentFont.Typeface.FamilyName);
			
			var info = new SKImageInfo(parameters.RenderWidth, parameters.RenderHeight);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.Clear(SKColors.Transparent);

				string secondText = secondValue.ToString();

				for (int i = parameters.DropShadowSize; i > 0; i--)
				{
					canvas.DrawText(
						secondText,
						new SKPoint(textPosition.X - i, textPosition.Y + i),
						SKTextAlign.Center,
						currentFont,
						shadowPaint);

					canvas.DrawText(
						secondText,
						new SKPoint(textPosition.X - i - 1, textPosition.Y + i),
						SKTextAlign.Center,
						currentFont,
						shadowPaint);
				}

				canvas.DrawText(
					secondText,
					textPosition,
					SKTextAlign.Center,
					currentFont,
					paint);

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}
}
