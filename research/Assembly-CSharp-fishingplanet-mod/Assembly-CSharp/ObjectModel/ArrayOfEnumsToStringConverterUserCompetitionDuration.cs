using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionDuration : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionDuration> list = new List<UserCompetitionDuration>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionDuration)Enum.Parse(typeof(UserCompetitionDuration), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionDuration)Enum.ToObject(typeof(UserCompetitionDuration), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
