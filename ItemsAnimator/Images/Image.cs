using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Images;

public abstract class Image
{
	protected static SKSvg LoadSVGResource(string name)
	{
		using (var resourceStream = typeof(Image).Assembly.GetManifestResourceStream("ItemsAnimator.Images.Resources." + name))
		{
			var svg = new SKSvg();

			svg.Load(resourceStream);

			return svg;
		}
	}

	public abstract void Render(SKCanvas target, SKPoint position, float t);
}
