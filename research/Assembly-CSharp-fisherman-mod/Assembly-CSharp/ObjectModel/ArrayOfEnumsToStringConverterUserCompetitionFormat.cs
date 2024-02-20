using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionFormat : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionFormat> list = new List<UserCompetitionFormat>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionFormat)Enum.Parse(typeof(UserCompetitionFormat), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionFormat)Enum.ToObject(typeof(UserCompetitionFormat), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
