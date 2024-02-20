using System;
using Newtonsoft.Json;

namespace ObjectModel.Tournaments
{
	public class TournamentRodEquipment
	{
		[JsonConverter(typeof(ArrayOfEnumsToStringConverterRodTemplate))]
		public RodTemplate[] RodBuilds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] RodTypes { get; set; }

		public int[] RodIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] LineTypes { get; set; }

		public int[] LineIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] TerminalTackleTypes { get; set; }

		public int[] TerminalTackleIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] SinkerTypes { get; set; }

		public int[] SinkerIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] FeederTypes { get; set; }

		public int[] FeederIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] LeaderTypes { get; set; }

		public int[] LeaderIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] HookTypes { get; set; }

		public int[] HookIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] BaitTypes { get; set; }

		public int[] BaitIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterBoilBaitForm))]
		public BoilBaitForm[] BoilBaitForms { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] ChumTypes { get; set; }

		public int[] ChumIds { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterChumCarpbaitsForm))]
		public ChumCarpbaitsForm[] ChumCarpbaitsForms { get; set; }

		[JsonIgnore]
		public bool HasRodCondition
		{
			get
			{
				return this.RodBuilds != null || this.RodTypes != null || this.RodIds != null;
			}
		}

		[JsonIgnore]
		public bool HasLineCondition
		{
			get
			{
				return this.LineTypes != null || this.LineIds != null;
			}
		}

		[JsonIgnore]
		public bool HasSinkerCondition
		{
			get
			{
				return this.SinkerTypes != null || this.SinkerIds != null;
			}
		}

		[JsonIgnore]
		public bool HasFeederCondition
		{
			get
			{
				return this.FeederTypes != null || this.FeederIds != null;
			}
		}

		[JsonIgnore]
		public bool HasLeaderCondition
		{
			get
			{
				return this.LeaderTypes != null || this.LeaderIds != null;
			}
		}

		[JsonIgnore]
		public bool HasTerminalTackleCondition
		{
			get
			{
				return this.TerminalTackleTypes != null || this.TerminalTackleIds != null;
			}
		}

		[JsonIgnore]
		public bool HasHookCondition
		{
			get
			{
				return this.HookTypes != null || this.HookIds != null;
			}
		}

		[JsonIgnore]
		public bool HasBaitCondition
		{
			get
			{
				return this.BaitTypes != null || this.BaitIds != null;
			}
		}

		[JsonIgnore]
		public bool HasChumCondition
		{
			get
			{
				return this.ChumTypes != null || this.ChumIds != null;
			}
		}
	}
}
