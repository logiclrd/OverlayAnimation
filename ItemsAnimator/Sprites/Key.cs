using SkiaSharp;

using Svg.Skia;

namespace ItemsAnimator.Sprites;

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text.RegularExpressions;
using ItemsAnimator.Utility;

public class Key : Sprite
{
	static SKSvg s_key;
	static SKSvg[] s_keyZapLeft;
	static SKSvg[] s_keyZapRight;

	static Key()
	{
		s_key = LoadSVGResource("key.svg");

		s_keyZapLeft = Enumerable.Range(1, 5).Select(n => LoadSVGResource($"key zap {n}.svg")).ToArray();
		s_keyZapRight = Enumerable.Range(6, 5).Select(n => LoadSVGResource($"key zap {n}.svg")).ToArray();
	}

	static Random s_rnd = new Random();

	public override float MaxWidth => 104;
	public override float MaxHeight => 140;

	class Zap
	{
		public SKSvg Visual;
		public SKPoint Offset;
	}

	const int ZapFrames = 15;

	List<Zap> _zaps = new List<Zap>();
	int _currentInstance;

	public override void Render(SKCanvas target, SKPoint position, float t)
	{
		float zapSeconds = ZapFrames / 23.976f;

		int instance = 0;

		while (t >= zapSeconds)
		{
			instance++;
			t -= zapSeconds;
		}

		if ((instance != _currentInstance) || (_zaps.Count == 0))
		{
			_zaps.Clear();

			var candidatesLeft = new List<SKSvg>(s_keyZapLeft);
			var candidatesRight = new List<SKSvg>(s_keyZapRight);

			void PickCandidate(List<SKSvg> candidates, List<Zap> zaps, int index, int side)
			{
				int candidateIndex = (int)Math.Floor(s_rnd.NextDouble() * candidatesLeft.Count);

				var zap = new Zap();

				zap.Visual = candidates[candidateIndex];
				zap.Offset = new SKPoint(index * side * 6, (float)(s_rnd.NextDouble() * 4 - 2));

				candidates.RemoveAt(candidateIndex);
				zaps.Add(zap);
			}

			for (int i=0; i < 3; i++)
			{
				PickCandidate(candidatesLeft, _zaps, i, -1);
				PickCandidate(candidatesRight, _zaps, i, +1);
			}

			_currentInstance = instance;
		}

		using (var transform = TransformScope.Translate(target, position))
		using (transform.Translate(-MaxWidth / 2, -MaxHeight / 2))
		using (transform.Scale(140f / 529.134f, 140f / 529.134f))
		{
			target.DrawPicture(s_key.Picture);

			int startIndex, count;

			if (t < 0.5)
			{
				t *= 2;

				startIndex = 0;
			}
			else
			{
				t = (t - 0.5f) * 2;

				startIndex = (int)Math.Floor(t * _zaps.Count / 2);;
			}

			count = 1 + (int)Math.Floor(t * _zaps.Count / 2);

			count *= 2; // Left and right
			
			for (int i = 0; i < count; i++)
			{
				int zapIndex = i + startIndex;

				var zap = _zaps[zapIndex];

				using (transform.Translate(zap.Offset))
					target.DrawPicture(zap.Visual.Picture);
			}
		}
	}
}
