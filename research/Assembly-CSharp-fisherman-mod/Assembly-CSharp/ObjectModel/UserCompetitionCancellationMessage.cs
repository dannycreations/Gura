using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class UserCompetitionCancellationMessage : UserCompetitionMessageBase
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionCancellationReason Reason { get; set; }

		public int Amount { get; set; }

		public string Currency { get; set; }
	}
}
