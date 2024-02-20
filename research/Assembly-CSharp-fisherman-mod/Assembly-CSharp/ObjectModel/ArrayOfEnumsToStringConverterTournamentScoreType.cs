using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterTournamentScoreType : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<TournamentScoreType> list = new List<TournamentScoreType>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((TournamentScoreType)Enum.Parse(typeof(TournamentScoreType), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((TournamentScoreType)Enum.ToObject(typeof(TournamentScoreType), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
