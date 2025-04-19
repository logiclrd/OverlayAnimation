using System;
using System.IO;

using SkiaSharp;

namespace CountdownAnimator;

class Program
{
	static void Main()
	{
		var countdown = new Countdown();

		var parameters = countdown.CreateDefaultParameters();

		var fonts = countdown.BuildFontList(parameters.FontDirectory);

		int frameNumber = 0;

		foreach (var frame in countdown.RenderSequence(parameters, fonts))
		{
			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);

			frameNumber++;
		}
	}
}
