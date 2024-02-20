using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionSortType : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionSortType> list = new List<UserCompetitionSortType>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionSortType)Enum.Parse(typeof(UserCompetitionSortType), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionSortType)Enum.ToObject(typeof(UserCompetitionSortType), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
