using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionType : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionType> list = new List<UserCompetitionType>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionType)Enum.Parse(typeof(UserCompetitionType), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionType)Enum.ToObject(typeof(UserCompetitionType), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
