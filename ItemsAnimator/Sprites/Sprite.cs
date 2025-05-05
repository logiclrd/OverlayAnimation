using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

public abstract class Sprite
{
	protected static SKSvg LoadSVGResource(string name)
	{
		using (var resourceStream = typeof(Sprite).Assembly.GetManifestResourceStream("ItemsAnimator.Sprites.Resources." + name))
		{
			var svg = new SKSvg();

			svg.Load(resourceStream);

			return svg;
		}
	}

	public abstract float MaxWidth { get; }
	public abstract float MaxHeight { get; }

	public abstract void Render(SKCanvas target, SKPoint position, float t);
}
