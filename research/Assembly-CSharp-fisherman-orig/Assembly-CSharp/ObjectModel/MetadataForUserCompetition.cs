using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class MetadataForUserCompetition
	{
		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionFormat))]
		public UserCompetitionFormat[] Formats { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionRewardScheme))]
		public UserCompetitionRewardScheme[] RewardSchemes { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionDuration))]
		public UserCompetitionDuration[] Durations { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionSortType))]
		public UserCompetitionSortType[] SortTypes { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionType))]
		public UserCompetitionType[] Types { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionRodEquipmentAllowed))]
		public UserCompetitionRodEquipmentAllowed[] RodEquipment { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterUserCompetitionEquipmentAllowed))]
		public UserCompetitionEquipmentAllowed[] DollEquipment { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterTournamentScoreType))]
		public TournamentScoreType[] ScoringTypes { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterTournamentFishSource))]
		public TournamentFishSource[] FishSources { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterTournamentFishOrigin))]
		public TournamentFishOrigin[] FishOrigins { get; set; }

		public MetadataForUserCompetitionPond[] Ponds { get; set; }

		public TournamentTemplateBrief[] Templates { get; set; }

		public string[] CurrenciesFee { get; set; }

		public int MinParticipants { get; set; }

		public int MaxParticipants { get; set; }

		public int MaxParticipantsDefault { get; set; }

		public int MinTeamParticipants { get; set; }

		public int MaxTeamParticipants { get; set; }

		public int MinTeams { get; set; }

		public int MaxTeams { get; set; }

		public int MinLevel { get; set; }

		public int MaxLevel { get; set; }

		public int MinEntranceFee { get; set; }

		public int MaxEntranceFee { get; set; }

		public string[] Teams { get; set; }

		public UserCompetition CompetitionTemplate { get; set; }
	}
}
