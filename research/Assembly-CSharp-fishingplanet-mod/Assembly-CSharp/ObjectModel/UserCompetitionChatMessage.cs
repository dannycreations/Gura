using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class UserCompetitionChatMessage
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public UserCompetitionChatMessageType MessageType { get; set; }

		public Guid PlayerId { get; set; }

		public string PlayerName { get; set; }

		public Guid? OtherPlayerId { get; set; }

		public string OtherPlayerName { get; set; }

		public int? FishId { get; set; }

		public float? FishWeight { get; set; }

		public string Team { get; set; }

		public List<UserCompetitionPlayer> Players { get; set; }

		public UserCompetitionRuntime UserCompetitionRuntime { get; set; }
	}
}
