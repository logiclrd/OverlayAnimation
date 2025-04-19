using System;
using System.IO;

using SkiaSharp;

namespace SwoopAnimator;

class Program
{
	static void Main()
	{
		var rnd = new Random();

		var swoop = new Swoop();

		var parameters = swoop.CreateDefaultParameters();

		parameters.RandomSeed = rnd.Next();

		int frameNumber = 0;

		foreach (var frame in swoop.RenderSequence(parameters))
		{
			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);

			frameNumber++;
		}
	}
}
