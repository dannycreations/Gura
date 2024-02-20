using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class TournamentScoring
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentScoreType ScoringType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentTotalScoreKind TotalScoreKind { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentFishSource ScoringFishSource { get; set; }

		public bool InheritLimits { get; set; }

		public ScoreFish[] FishScore { get; set; }

		public int[] FishCategories { get; set; }

		public float? MinWeight { get; set; }

		public float? MaxWeight { get; set; }

		public float? ReferenceWeight { get; set; }

		public int? FishCount { get; set; }

		public int? ScorePerFish { get; set; }

		public int? ScorePerFishType { get; set; }

		public int? ScorePerKg { get; set; }

		public int? ScorePerMeter { get; set; }

		public TournamentFishOrigin FishOrigin { get; set; }

		[JsonIgnore]
		public bool MaxScoreWins
		{
			get
			{
				return this.ScoringType != TournamentScoreType.SmallestFish && this.ScoringType != TournamentScoreType.BestWeightMatch;
			}
		}

		[JsonIgnore]
		public bool OneFishWins
		{
			get
			{
				return this.ScoringType == TournamentScoreType.SmallestFish || this.ScoringType == TournamentScoreType.BiggestFish || this.ScoringType == TournamentScoreType.BestWeightMatch || this.ScoringType == TournamentScoreType.BiggestSizeDiff || this.ScoringType == TournamentScoreType.LongestFish;
			}
		}

		[JsonIgnore]
		public bool SumOfTop3ScoreWins
		{
			get
			{
				return this.ScoringType == TournamentScoreType.TotalLengthTop3;
			}
		}
	}
}
