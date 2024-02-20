using System;

namespace ObjectModel
{
	public class TopPlayerBase : ILevelRank
	{
		public Guid UserId { get; set; }

		public string UserName { get; set; }

		public int AvatarBID { get; set; }

		public string AvatarUrl { get; set; }

		public string ExternalId { get; set; }

		public int Level { get; set; }

		public int Rank { get; set; }

		public long Experience { get; set; }

		public long RankExperience { get; set; }
	}
}
