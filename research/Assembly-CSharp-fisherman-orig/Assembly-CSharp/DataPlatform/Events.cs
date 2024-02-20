using System;
using System.Diagnostics;

namespace DataPlatform
{
	public class Events
	{
		[Conditional("UNITY_XBOXONE")]
		public static void SendArrivedToBase(string UserId, ref Guid PlayerSessionId, int RandomMessageId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendArrivedToPond(string UserId, ref Guid PlayerSessionId, int PondId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendCompetitionsPlayed(string UserId, ref Guid PlayerSessionId, int CompetitionsPlayed)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendExperience(string UserId, ref Guid PlayerSessionId, ulong Experience)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendFishCaught(string UserId, ref Guid PlayerSessionId, int FishCount)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendUniqueFishCaught(string UserId, ref Guid PlayerSessionId, int UniqueFishCaught)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPremiumFishCaught(string UserId, ref Guid PlayerSessionId, int PremiumFishCount)
		{
		}
	}
}
