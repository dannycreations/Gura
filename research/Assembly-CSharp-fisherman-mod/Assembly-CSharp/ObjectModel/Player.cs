using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	[JsonObject]
	public class Player : IEqualityComparer<Player>, IPlayer, ILevelRank
	{
		public string UserId { get; set; }

		public string UserName { get; set; }

		public int? AvatarBID { get; set; }

		public string AvatarUrl { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FriendStatus Status { get; set; }

		public DateTime? RequestTime { get; set; }

		public bool IsOnline { get; set; }

		public int Level { get; set; }

		public int Rank { get; set; }

		public int? PondId { get; set; }

		public bool HasPremium { get; set; }

		public string Gender { get; set; }

		public string Source { get; set; }

		public string ExternalId { get; set; }

		public string RoomId { get; set; }

		[JsonIgnore]
		public TPMCharacterModel TpmCharacterModel { get; set; }

		[JsonIgnore]
		public bool IsReplay
		{
			get
			{
				return false;
			}
		}

		public bool Equals(Player x, Player y)
		{
			return x.GetHashCode(x) == y.GetHashCode(y);
		}

		public int GetHashCode(Player obj)
		{
			return ((obj.UserId == null) ? 0 : obj.UserId.GetHashCode()) ^ obj.Status.GetHashCode();
		}
	}
}
