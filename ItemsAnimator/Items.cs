using System.Collections.Generic;

using SkiaSharp;

using ItemsAnimator.Images;

namespace ItemsAnimator;

public class Items
{
	public Parameters CreateDefaultParameters()
	{
		return
			new Parameters()
			{
				RenderWidth = 600,
				RenderHeight = 600,
			};
	}

	public IEnumerable<SKBitmap> RenderSequence(Parameters parameters)
	{
		for (int frameNumber = 0; frameNumber < 60; frameNumber++)
		{
			float t = frameNumber / 23.976f;

			var info = new SKImageInfo(parameters.RenderWidth, parameters.RenderHeight);
		
			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				var item = new Tealight();

				item.Render(canvas, new SKPoint(300, 300), t);

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}
}
