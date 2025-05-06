using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using ItemsAnimator.Utility;

public class BubbleWand : Sprite
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

	public override float MaxWidth => 135;
	public override float MaxHeight => 180;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		while (t >= 1)
			t -= 1;

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		using (transform.Scale(0.7317f, 0.7317f))
		{
			using (transform.Translate(0, 124))
				target.DrawPicture(s_wand.Picture);

			if (t > 0.25f)
			{
				using (transform.Translate(61, 56))
					target.DrawPicture(s_bubbleLarge.Picture);
			}

			if (t > 0.5f)
			{
				using (transform.Translate(125, 11))
					target.DrawPicture(s_bubbleMedium.Picture);
			}

			if (t > 0.75f)
			{
				using (transform.Translate(19, 0))
					target.DrawPicture(s_bubbleSmol.Picture);
			}
		}
	}
}
