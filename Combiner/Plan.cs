using System.Collections.Generic;

namespace Combiner;

public class Plan
{
	public string OutputDirectoryName;

	public int RenderWidth;
	public int RenderHeight;
	public int RenderFrames;

	public bool OutputBlankFrames = true;
	public bool ClearOutputDirectory = true;

	public int SequenceFadeOutFrames;

	public float FramesPerSecond;

	public IEnumerable<Sequence> Sequences;
}
