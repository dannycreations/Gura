using System;
using ObjectModel;

namespace BiteEditor
{
	public struct AttractorData
	{
		public AttractorData(IAttractor attractor, Random rnd)
		{
			this.FillTime = attractor.FillTime;
			this.AttractionMinValue = attractor.AttractionMinValue;
			this.AttractionMaxValue = attractor.AttractionMaxValue;
			this.MaxPowerDuration = attractor.MaxPowerDuration;
			this.AttractionR = attractor.AttractionR;
			this.Position2D = attractor.Position2D;
			if (attractor.RandomR > 0f)
			{
				this.Position2D += new Vector2f((float)((rnd.NextDouble() * 2.0 - 1.0) * (double)attractor.RandomR), (float)((rnd.NextDouble() * 2.0 - 1.0) * (double)attractor.RandomR));
			}
		}

		public readonly int FillTime;

		public readonly float AttractionMinValue;

		public readonly float AttractionMaxValue;

		public readonly float AttractionR;

		public readonly Vector2f Position2D;

		public readonly int MaxPowerDuration;
	}
}
