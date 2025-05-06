using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SkiaSharp;

namespace Combiner;

using SwoopAnimator;
using BugsAnimator;
using CountdownAnimator;
using ItemsAnimator.Sprites;

class Program
{
	static Sequence DrHorrible_Moustache_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 9301;
		sequence.CountdownSeconds = 10;
		sequence.Sprite = new Moustache();

		return sequence;
	}

	static Sequence DrHorrible_Key_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 38002;
		sequence.CountdownSeconds = 10;
		sequence.Sprite = new Key();

		return sequence;
	}

	static Sequence DrHorrible_Cookie_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 44012;
		sequence.CountdownSeconds = 10;
		sequence.Sprite = new Cookie();

		return sequence;
	}

	static Sequence DrHorrible_FingerMonster_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 60817;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new FingerMonster();
		sequence.SpritePositionTweak = new SKPoint(-30, 0);

		return sequence;
	}

	static Sequence Buffy_Dinos_Sequence()
	{
		// I gave birth to a pterodactyl
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 11238;
		sequence.CountdownSeconds = 10;
		sequence.Sprite = new Dinos();

		return sequence;
	}

	static Sequence Buffy_BubbleWand_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 13371;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new BubbleWand();
		sequence.SpritePositionTweak = new SKPoint(20, 0);

		return sequence;
	}

	static Sequence Buffy_Lips_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 38020;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new Lips();

		return sequence;
	}

	static Sequence Buffy_Tealight_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 51931;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new Tealight();
		sequence.SpritePositionTweak = new SKPoint(-20, 0);

		return sequence;
	}

	static Sequence Buffy_Noisemaker_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 70356;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new Noisemaker();

		return sequence;
	}

	static Sequence Buffy_FingerMonster_Sequence()
	{
		var sequence = new Sequence();

		sequence.FinalFrameNumber = 71984;
		sequence.CountdownSeconds = 20;
		sequence.Sprite = new FingerMonster();
		sequence.SpritePositionTweak = new SKPoint(-30, 0);

		return sequence;
	}

	static IEnumerable<Sequence> CreateDrHorribleSequences()
	{
		yield return DrHorrible_Moustache_Sequence();
		yield return DrHorrible_Key_Sequence();
		yield return DrHorrible_FingerMonster_Sequence();
	}

	static IEnumerable<Sequence> CreateBuffySequences()
	{
		yield return Buffy_Dinos_Sequence();
		yield return Buffy_BubbleWand_Sequence();
		yield return Buffy_Lips_Sequence();
		yield return Buffy_Tealight_Sequence();
		yield return Buffy_Noisemaker_Sequence();
		yield return Buffy_FingerMonster_Sequence();
	}

	static Plan CreateDrHorriblePlan()
	{
		var plan = new Plan();

		plan.OutputDirectoryName = "Output/DrHorrible";
		plan.RenderWidth = 1920;
		plan.RenderHeight = 1080;
		plan.FramesPerSecond = 23.976f;
		plan.SequenceFadeOutFrames = 15;
		plan.Sequences = CreateDrHorribleSequences();

		return plan;
	}

	static Plan CreateBuffyPlan()
	{
		var plan = new Plan();

		plan.OutputDirectoryName = "Output/Buffy";
		plan.RenderWidth = 1920;
		plan.RenderHeight = 1080;
		plan.FramesPerSecond = 23.976f;
		plan.SequenceFadeOutFrames = 15;
		plan.Sequences = CreateBuffySequences();

		return plan;
	}

	static void Main()
	{
		ExecutePlan(CreateDrHorriblePlan());
		ExecutePlan(CreateBuffyPlan());
	}

	static void ExecutePlan(Plan plan)
	{
		int nextOutputFrameNumber = 0;

		var swoop = new Swoop();
		var bugs = new Bugs();
		var countdown = new Countdown();

		var swoopParameters = swoop.CreateDefaultParameters();
		var bugsParameters = bugs.CreateDefaultParameters();
		var countdownParameters = countdown.CreateDefaultParameters();

		Directory.CreateDirectory(plan.OutputDirectoryName);

		if (plan.ClearOutputDirectory)
		{
			var deletePaths = new List<string>();

			foreach (var existingFilePath in Directory.EnumerateFiles(plan.OutputDirectoryName))
			{
				string existingFileName = Path.GetFileName(existingFilePath);

				if (existingFileName.StartsWith("frame") && existingFileName.EndsWith(".png"))
					deletePaths.Add(existingFilePath);
			}

			Console.WriteLine("Deleting {0} files of previous output", deletePaths.Count);

			deletePaths.ForEach(File.Delete);
		}

		foreach (var sequence in plan.Sequences)
		{
			countdownParameters.CountdownSeconds = sequence.CountdownSeconds;

			int countdownFrames = (int)Math.Ceiling(sequence.CountdownSeconds * plan.FramesPerSecond);
			int swoopFrames = (int)Math.Ceiling(swoopParameters.AnimationDuration * plan.FramesPerSecond);

			int sequenceStartFrameNumber = sequence.FinalFrameNumber - countdownFrames - swoopFrames + OverlapFrameCount;

			if (plan.OutputBlankFrames)
				while (nextOutputFrameNumber < sequenceStartFrameNumber)
					OutputBlankFrame(plan, nextOutputFrameNumber++);
			else
				nextOutputFrameNumber = sequenceStartFrameNumber;

			var fonts = countdown.BuildFontList(countdownParameters);

			var swoopGenerator = swoop.RenderSequence(swoopParameters);
			var bugsGenerator = bugs.RenderSequence(bugsParameters);
			var countdownGenerator = countdown.RenderSequence(countdownParameters, fonts);

			foreach (var frame in RenderSequence(swoopGenerator, bugsGenerator, countdownGenerator, sequence.Sprite, bugsParameters.CornerRadius, sequence.SpritePositionTweak))
				OutputFrame(frame, plan, nextOutputFrameNumber++);
		}

		if (plan.OutputBlankFrames)
			OutputBlankFrame(plan, nextOutputFrameNumber++);

		Console.WriteLine("===========================");

		int sequenceNumber = 1;

		foreach (var sequence in plan.Sequences)
		{
			var spriteName = sequence.Sprite.GetType().Name;

			var startTime = TimeSpan.FromSeconds(sequence.FinalFrameNumber / plan.FramesPerSecond);

			Console.WriteLine("Sequence {0}: {1} at {2}", sequenceNumber, spriteName, startTime);

			sequenceNumber++;
		}
	}

	static SKBitmap CreateBlankFrame(Plan plan)
	{
		return new SKBitmap(plan.RenderWidth, plan.RenderHeight, isOpaque: false);
	}

	static byte[] s_blankFrameBytes;

	static void OutputBlankFrame(Plan plan, int frameNumber)
	{
		if (s_blankFrameBytes == null)
		{
			var buffer = new MemoryStream();

			SaveFrameTo(CreateBlankFrame(plan ), buffer);

			s_blankFrameBytes = buffer.ToArray();
		}

		Console.WriteLine("OUTPUT BLANK FRAME: {0}", frameNumber);

		string frameFileName = "frame" + frameNumber.ToString("d5") + ".png";

		string outputPath = Path.Combine(plan.OutputDirectoryName, frameFileName);

		File.WriteAllBytes(outputPath, s_blankFrameBytes);
	}

	static void OutputFrame(SKBitmap frame, Plan plan, int frameNumber)
	{
		Console.WriteLine("OUTPUT FRAME: {0}", frameNumber);

		string frameFileName = "frame" + frameNumber.ToString("d5") + ".png";

		string outputPath = Path.Combine(plan.OutputDirectoryName, frameFileName);

		using (var stream = File.OpenWrite(outputPath))
			SaveFrameTo(frame, stream);
	}

	static void SaveFrameTo(SKBitmap frame, Stream target)
	{
		var imageData = frame.Encode(SKEncodedImageFormat.Png, default);

		imageData.SaveTo(target);
	}

	static SKBitmap ExtractAlpha(SKBitmap bmp)
	{
		var info = new SKImageInfo(bmp.Width, bmp.Height);

		using (var surface = SKSurface.Create(info))
		{
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Black);

			var image = surface.Snapshot();

			var alpha = SKBitmap.FromImage(image);

			byte[] imageBytes = bmp.Bytes;
			byte[] alphaBytes = new byte[imageBytes.Length];

			for (int y = 0, o = 0; y < bmp.Height; y++)
				for (int x = 0; x < bmp.Width; x++, o += 4)
				{
					byte opacity = imageBytes[o + 3];

					alphaBytes[o + 0] = alphaBytes[o + 1] = alphaBytes[o + 2] = opacity;
					alphaBytes[o + 3] = 255;
				}

			unsafe
			{
				fixed (byte *alphaBytePtr = &alphaBytes[0])
				{
					alpha.InstallPixels(alpha.Info, new IntPtr(alphaBytePtr), alpha.Width * 4);
				}
			}


			return alpha;
		}
	}

	static void MultiplyAlpha(SKBitmap from, SKBitmap to, float multiplicand)
	{
		byte[] imageBytes = from.Bytes;

		for (int y = 0, o = 0; y < from.Height; y++, o += from.RowBytes)
			for (int x = 0, p = o; x < from.Width; x++, p += from.BytesPerPixel)
			{
				byte opacity = imageBytes[p + 3];

				opacity = (byte)Math.Max(0, Math.Min(255, Math.Round(opacity * multiplicand)));

				imageBytes[p + 3] = opacity;
			}

		unsafe
		{
			fixed (byte *imageBytePtr = &imageBytes[0])
			{
				to.InstallPixels(to.Info, new IntPtr(imageBytePtr), from.RowBytes);
			}
		}
	}

	const int OverlapFrameCount = 5;
	const int ReminderFadeInFrames = 8;
	const int ReminderFadeOutFrames = 20;

	static IEnumerable<SKBitmap> RenderSequence(IEnumerable<SKBitmap> swoopGenerator, IEnumerable<SKBitmap> bugsGenerator, IEnumerable<SKBitmap> countdownGenerator, Sprite sprite, float reminderCornerRadius, SKPoint spritePositionTweak)
	{
		// Animation sequence
		// * Take all swoop frames
		// * Fade bugs in with countdown overlayed starting 5 frames from the end
		// * Hold on last swoop frame until countdown has exhausted frames
		// * Fade everything out over 0.5s

		var swoopFrames = swoopGenerator.ToList();

		var reminderFrames = CombineFrames(bugsGenerator, countdownGenerator).ToList();

		for (int i=0; i < swoopFrames.Count - OverlapFrameCount; i++)
			yield return swoopFrames[i];

		SKBitmap finalFrame = null;

		for (int i=0; i < reminderFrames.Count; i++)
		{
			bool isLastFrame = (i + 1 == reminderFrames.Count);

			int swoopFrameIndex = swoopFrames.Count - OverlapFrameCount + i;

			if (swoopFrameIndex >= swoopFrames.Count)
				swoopFrameIndex = swoopFrames.Count - 1;

			float time = i / 23.976f;

			var swoopFrame = swoopFrames[swoopFrameIndex];
			var reminderFrame = reminderFrames[i];

			var info = new SKImageInfo(swoopFrame.Width, swoopFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					swoopFrame,
					new SKPoint(0, 0),
					new SKPaint());

				float opacity;

				if (i < ReminderFadeInFrames)
					opacity = i / (float)ReminderFadeInFrames;
				else
					opacity = 1.0f;

				byte opacityByte;

				if (opacity < 0)
					opacityByte = 0;
				else if (opacity > 1.0f)
					opacityByte = 255;
				else
					opacityByte = (byte)(255 * opacity);

				float reminderX = swoopFrame.Width - reminderFrame.Width * 1.25f;
				float reminderY = 20;

				canvas.DrawBitmap(
					reminderFrame,
					new SKPoint(reminderX, reminderY),
					new SKPaint()
					{
						Color = SKColors.White.WithAlpha(opacityByte)
					});

				var clipArea = new SKRoundRect();

				var cornerRadius = new SKPoint(reminderCornerRadius - 4, reminderCornerRadius - 4);

				clipArea.SetRectRadii(
					new SKRect(reminderX + 4, reminderY + 4, reminderX + reminderFrame.Width - 8, reminderY + reminderFrame.Height - 8),
					new SKPoint[]
					{
						cornerRadius,
						cornerRadius,
						cornerRadius,
						cornerRadius,
					});

				canvas.ClipRoundRect(clipArea);

				sprite.Render(
					canvas,
					new SKPoint(reminderX + 170 + sprite.MaxWidth / 2 + spritePositionTweak.X, reminderY + reminderFrame.Height / 2 + spritePositionTweak.Y),
					time);

				var image = surface.Snapshot();

				var frameBitmap = SKBitmap.FromImage(image);

				if (isLastFrame)
					finalFrame = frameBitmap;

				yield return frameBitmap;
			}
		}

		for (int i=0; i < ReminderFadeOutFrames; i++)
		{
			byte opacity = (byte)(255 - i * 255 / ReminderFadeOutFrames);

			var opacityPaint =
				new SKPaint()
				{
					Color = SKColors.White.WithAlpha(opacity)
				};

			var info = new SKImageInfo(finalFrame.Width, finalFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					finalFrame,
					new SKPoint(0, 0),
					opacityPaint);

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}

	static IEnumerable<SKBitmap> CombineFrames(IEnumerable<SKBitmap> bugsGenerator, IEnumerable<SKBitmap> countdownGenerator)
	{
		var bugsFrames = bugsGenerator.GetEnumerator();
		var countdownFrames = countdownGenerator.GetEnumerator();

		while (bugsFrames.MoveNext() && countdownFrames.MoveNext())
		{
			var bugsFrame = bugsFrames.Current;
			var countdownFrame = countdownFrames.Current;

			var info = new SKImageInfo(bugsFrame.Width, bugsFrame.Height);

			using (var surface = SKSurface.Create(info))
			{
				var canvas = surface.Canvas;

				canvas.DrawBitmap(
					bugsFrame,
					new SKPoint(0, 0),
					new SKPaint());

				canvas.DrawBitmap(
					countdownFrame,
					new SKPoint((bugsFrame.Width / 2 - countdownFrame.Width) / 2, -20),
					new SKPaint());

				var image = surface.Snapshot();

				yield return SKBitmap.FromImage(image);
			}
		}
	}
}
