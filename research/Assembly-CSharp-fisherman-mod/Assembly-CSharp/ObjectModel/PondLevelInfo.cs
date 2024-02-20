using System;

namespace ObjectModel
{
	public class PondLevelInfo
	{
		public int PondId { get; set; }

		public int MinLevel { get; set; }

		public DateTime? MinLevelExpiration { get; set; }
	}
}
