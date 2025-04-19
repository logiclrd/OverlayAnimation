using System;
using System.IO;
using System.Linq;

namespace BugsAnimator;

using SkiaSharp;

class Program
{
	static void Main()
	{
		var bugs = new Bugs();

		var parameters = bugs.CreateDefaultParameters();

		int frameNumber = 0;

		foreach (var frame in bugs.RenderSequence(parameters))
		{
			var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

			using (var stream = File.OpenWrite("frame" + frameNumber.ToString("d4") + ".png"))
				imageData.SaveTo(stream);

			frameNumber++;
		}
	}
}
