using System;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class Range
	{
		public Range(int min, int max)
		{
			this.Max = max;
			this.Min = min;
		}

		public Range(int min, int max, bool checkMin, bool checkMax)
			: this(min, max)
		{
			this.CheckMin = checkMin;
			this.CheckMax = checkMax;
		}

		public Range(Range r)
		{
			this.Max = r.Max;
			this.Min = r.Min;
		}

		public int Max { get; private set; }

		public int Min { get; private set; }

		public bool Check(int v)
		{
			return v >= this.Min && v <= this.Max;
		}

		public bool Check(string v)
		{
			int num;
			return int.TryParse(v, out num) && (num >= this.Min || !this.CheckMin) && (num <= this.Max || !this.CheckMax);
		}

		protected readonly bool CheckMin = true;

		protected readonly bool CheckMax = true;
	}
}
