using SkiaSharp;

namespace Combiner;

public class Sequence
{
	public int FinalFrameNumber;
	public int CountdownSeconds;
	public ItemsAnimator.Sprites.Sprite Sprite;
	public SKPoint SpritePositionTweak = SKPoint.Empty;
}
