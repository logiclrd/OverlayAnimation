using System;
using System.IO;

using SkiaSharp;

namespace ItemsAnimator;

class Program
{
	static void Main()
	{
		var rnd = new Random();

		var items = new Items();

		var parameters = items.CreateDefaultParameters();

		int frameNumber = 0;

		foreach (var frame in items.RenderSequence(parameters))
		{
			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);

			frameNumber++;
		}
	}
}
