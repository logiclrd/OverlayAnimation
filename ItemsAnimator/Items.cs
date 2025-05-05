using System;
using System.Collections.Generic;

using SkiaSharp;

using ItemsAnimator.Sprites;

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
		var item = new Tealight();

		parameters.RenderWidth = (int)Math.Ceiling(item.MaxWidth);
		parameters.RenderHeight = (int)Math.Ceiling(item.MaxHeight);

		for (int frameNumber = 0; frameNumber < 60; frameNumber++)
		{
			float t = frameNumber / 23.976f;

			var info = new SKImageInfo(parameters.RenderWidth, parameters.RenderHeight);
		
			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				item.Render(canvas, new SKPoint(item.MaxWidth / 2, item.MaxHeight / 2), t);

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}
}
