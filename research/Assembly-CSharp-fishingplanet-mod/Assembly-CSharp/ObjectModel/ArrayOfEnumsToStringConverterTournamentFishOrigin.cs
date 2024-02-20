using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterTournamentFishOrigin : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<TournamentFishOrigin> list = new List<TournamentFishOrigin>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((TournamentFishOrigin)Enum.Parse(typeof(TournamentFishOrigin), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((TournamentFishOrigin)Enum.ToObject(typeof(TournamentFishOrigin), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
