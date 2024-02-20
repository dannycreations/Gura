using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FilterForUserCompetitions
	{
		public Guid RequestId { get; set; }

		public int Skip { get; set; }

		public int Take { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public OrderForUserCompetitions Order { get; set; }

		public DateTime? From { get; set; }

		public DateTime? To { get; set; }

		public bool DontShowManualStart { get; set; }

		public bool ShowEnded { get; set; }

		public bool IamRegisteredIn { get; set; }

		public bool IamStillParticipating { get; set; }

		public Guid? HostUserId { get; set; }

		public int[] PondIds { get; set; }

		public int[] FishCategories { get; set; }

		public string[] FishTypes { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public UserCompetitionFormat[] Formats { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public TournamentScoreType[] ScoreTypes { get; set; }

		public string Currency { get; set; }

		public bool? IsSponsored { get; set; }

		public bool? IsPublished { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionApproveStatus? ApproveStatus { get; set; }
	}
}
