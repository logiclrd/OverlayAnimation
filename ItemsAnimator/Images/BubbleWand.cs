using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Images;

using ItemsAnimator.Utility;

public class BubbleWand : Image
{
	static SKSvg s_wand;
	static SKSvg s_bubbleSmol;
	static SKSvg s_bubbleMedium;
	static SKSvg s_bubbleLarge;

	static BubbleWand()
	{
		s_wand = LoadSVGResource("bubble wand.svg");
		s_bubbleSmol = LoadSVGResource("bubble smol.svg");
		s_bubbleMedium = LoadSVGResource("bubble medium.svg");
		s_bubbleLarge = LoadSVGResource("bubble large.svg");
	}

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= 1)
			t -= 1;

		using (var transform = TransformScope.Translate(target, -137, -123))
		{
			using (transform.Translate(41, 124))
				target.DrawPicture(s_wand.Picture, position);

			if (t > 0.25f)
			{
				using (transform.Translate(60, 0))
					target.DrawPicture(s_bubbleSmol.Picture, position);
			}

			if (t > 0.5f)
			{
				using (transform.Translate(166, 11))
					target.DrawPicture(s_bubbleMedium.Picture, position);
			}

			if (t > 0.75f)
			{
				using (transform.Translate(102, 56))
					target.DrawPicture(s_bubbleLarge.Picture, position);
			}
		}
	}
}
