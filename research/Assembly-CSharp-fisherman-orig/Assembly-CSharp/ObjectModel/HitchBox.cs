using System;

namespace ObjectModel
{
	public class HitchBox
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public Point3 Position { get; set; }

		public Point3 Scale { get; set; }

		public Point3 Rotation { get; set; }

		public float HitchProbability { get; set; }

		public bool CanBreak { get; set; }

		public float MaxLoad { get; set; }

		public GeneratedItem[] GeneratedItems { get; set; }
	}
}
