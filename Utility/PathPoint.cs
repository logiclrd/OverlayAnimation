using SkiaSharp;

namespace Utility;

public class PathPoint
{
	public int FrameNumber;
	public SKPoint Offset;
	public bool Smooth;

	public PathPoint() {}

	public PathPoint(int frameNumber, SKPoint offset, bool smooth = false)
	{
		this.FrameNumber = frameNumber;
		this.Offset = offset;
		this.Smooth = smooth;
	}

	public PathPoint(int frameNumber, float value, bool smooth = false)
		: this(frameNumber, new SKPoint(value, value), smooth)
	{
	}
}
