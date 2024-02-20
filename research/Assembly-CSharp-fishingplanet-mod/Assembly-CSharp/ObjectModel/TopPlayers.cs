using System;

namespace ObjectModel
{
	public class TopPlayers : TopPlayerBase
	{
		public int? PondId { get; set; }

		public string PondName { get; set; }

		public int DaysInGame { get; set; }

		public int FishCount { get; set; }

		public int TrophyCount { get; set; }

		public int UniqueCount { get; set; }

		public long WeeklyExpGain { get; set; }
	}
}
