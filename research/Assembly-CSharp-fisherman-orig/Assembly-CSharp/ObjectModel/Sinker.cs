using System;

namespace ObjectModel
{
	public class Sinker : TerminalTackleItem, ISinker
	{
		public float SpeedIncBuoyancy { get; set; }

		public float MinReelSpeed { get; set; }

		public float MaxReelSpeed { get; set; }

		public float HitchChance { get; set; }
	}
}
