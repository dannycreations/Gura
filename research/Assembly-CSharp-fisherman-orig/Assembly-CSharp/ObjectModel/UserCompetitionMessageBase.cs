using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public abstract class UserCompetitionMessageBase
	{
		public int TournamentId { get; set; }

		public string NameCustom { get; set; }

		public Guid? HostUserId { get; set; }

		public string HostName { get; set; }

		public int PondId { get; set; }

		public string PondName { get; set; }

		public DateTime? FixedStartDate { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionFormat Format { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionType Type { get; set; }

		public string TemplateName { get; set; }

		public bool IsSponsored { get; set; }
	}
}
