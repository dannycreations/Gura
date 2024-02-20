using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ObjectModel.Tournaments;

namespace ObjectModel
{
	public class UserCompetitionPublic : UserCompetitionRuntime
	{
		public static int UgcMaxDaysScheduledFuture { get; set; }

		public static int UgcCompetitionFeeCommission { get; set; }

		public static int UgcTicksCountBeforeStart { get; set; }

		public Guid? HostUserId { get; set; }

		public string HostName { get; set; }

		public string NameCustom { get; set; }

		public bool IsSponsored { get; set; }

		public UserCompetitionApproveStatus ApproveStatus { get; set; }

		public DateTime? SendToReviewDate { get; set; }

		public string SendToReviewComment { get; set; }

		public DateTime? StartReviewDate { get; set; }

		public DateTime? ReviewDate { get; set; }

		public string ReviewComment { get; set; }

		public Reward[] Rewards { get; set; }

		public bool IsPublished { get; set; }

		public DateTime? PublishDate { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionType Type { get; set; }

		public int? ImageBID { get; set; }

		public int? LogoBID { get; set; }

		public string TemplateName { get; set; }

		public string Rules { get; set; }

		public int PondId { get; set; }

		public string PondName { get; set; }

		public int ComissionPct { get; set; }

		public double? HostEntranceFee { get; set; }

		public double? EntranceFee { get; set; }

		public string Currency { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionFormat Format { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionRewardScheme RewardScheme { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionSortType SortType { get; set; }

		public string WeatherName { get; set; }

		public MetadataForUserCompetitionPondWeather.DayWeather[] DayWeathers { get; set; }

		public MetadataForUserCompetitionPondWeather.DayWeather SelectedDayWeather { get; set; }

		public string[] FishScore { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentFishSource FishSource { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public UserCompetitionRodEquipmentAllowed[] RodEquipment { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public UserCompetitionEquipmentAllowed[] DollEquipment { get; set; }

		public TournamentEquipment TournamentEquipment { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentScoreType ScoringType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentScoreType SecondaryScoringType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentTotalScoreKind TotalScoreKind { get; set; }

		public int? FishCount { get; set; }

		public float? ReferenceWeight { get; set; }

		public float? ScoringMinWeight { get; set; }

		public float? ScoringMaxWeight { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentFishOrigin FishOrigin { get; set; }

		public int? InGameStartHour { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionDuration Duration { get; set; }

		public DateTime? FixedStartDate { get; set; }

		public int? MinParticipants { get; set; }

		public int? MaxParticipants { get; set; }

		public int? MinLevel { get; set; }

		public int? MaxLevel { get; set; }

		public bool IsPrivate { get; set; }

		public string[] Teams { get; set; }

		public List<UserCompetitionPlayer> Players { get; set; }

		public bool IsActive { get; set; }

		public bool IsRegistered { get; set; }

		public bool IsApproved { get; set; }

		public bool IsDone { get; set; }

		public bool IsDisqualified { get; set; }

		public int RegistrationsCount { get; set; }

		public int ApprovedCount { get; set; }

		public int PlayingCount { get; set; }

		public int FinishedCount { get; set; }
	}
}
