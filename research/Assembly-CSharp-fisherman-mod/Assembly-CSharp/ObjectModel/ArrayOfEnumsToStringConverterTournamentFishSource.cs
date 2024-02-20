using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterTournamentFishSource : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<TournamentFishSource> list = new List<TournamentFishSource>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((TournamentFishSource)Enum.Parse(typeof(TournamentFishSource), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((TournamentFishSource)Enum.ToObject(typeof(TournamentFishSource), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
