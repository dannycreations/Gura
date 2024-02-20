using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Fish : IFish
	{
		public int FishId { get; set; }

		public Guid? InstanceId { get; set; }

		public string CodeName { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public FishTypes Type { get; set; }

		public int CategoryId { get; set; }

		public string Asset { get; set; }

		public float Weight { get; set; }

		public float Length { get; set; }

		public bool? IsYoung { get; set; }

		public bool? IsTrophy { get; set; }

		public bool? IsUnique { get; set; }

		public bool? IsPersonalRecord { get; set; }

		public float? Stamina { get; set; }

		public float? Activity { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FishBehavior? Behavior { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FishAttackStyle? AttackVector { get; set; }

		public float? SilverCost { get; set; }

		public float? GoldCost { get; set; }

		public float? Experience { get; set; }

		public float? BaseExperience { get; set; }

		public float? Force { get; set; }

		public float? Speed { get; set; }

		public FishPortrait Portrait { get; set; }

		public float? BiteTime { get; set; }

		public float? AttackLure { get; set; }

		public int? ThumbnailBID { get; set; }

		public float? TournamentScore { get; set; }

		public float? TournamentSecondaryScore { get; set; }

		public bool NoRelease { get; set; }

		public float? StaminaLoseMultiplier { get; set; }

		public float? StaminaGainMultiplier { get; set; }

		public float? StaminaGainFraction { get; set; }

		public float? AttackDelay { get; set; }

		public float? HoldDelay { get; set; }

		public float? EndGameDelay { get; set; }

		public bool IsActive { get; set; }

		public bool IsEvent { get; set; }

		public override string ToString()
		{
			return string.Format("#{0} '{1}' {2}, {3}kg, {4}m, Young:{5}, Trophy:{6}, Unique:{7} ", new object[]
			{
				this.FishId,
				this.CodeName,
				this.Name,
				this.Weight.ToString(CultureInfo.InvariantCulture),
				this.Length.ToString(CultureInfo.InvariantCulture),
				(this.IsYoung == null || !this.IsYoung.Value) ? "N" : "Y",
				(this.IsTrophy == null || !this.IsTrophy.Value) ? "N" : "Y",
				(this.IsUnique == null || !this.IsUnique.Value) ? "N" : "Y"
			});
		}
	}
}
