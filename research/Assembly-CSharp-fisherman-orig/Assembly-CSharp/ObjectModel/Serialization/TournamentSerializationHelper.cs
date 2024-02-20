using System;
using Newtonsoft.Json;

namespace ObjectModel.Serialization
{
	public static class TournamentSerializationHelper
	{
		public static string SerializeTournamentResult(TournamentFinalResult tournamentFinalResult)
		{
			return string.Format("{0}{1}", 'R', JsonConvert.SerializeObject(tournamentFinalResult, 0, new JsonSerializerSettings
			{
				NullValueHandling = 1
			}));
		}

		public static void DeserializeTournamentResult(string data, out TournamentFinalResult tournamentFinalResult)
		{
			string text = data.Substring(1);
			tournamentFinalResult = JsonConvert.DeserializeObject<TournamentFinalResult>(text);
		}
	}
}
