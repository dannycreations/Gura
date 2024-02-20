using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionRewardScheme : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionRewardScheme> list = new List<UserCompetitionRewardScheme>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionRewardScheme)Enum.Parse(typeof(UserCompetitionRewardScheme), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionRewardScheme)Enum.ToObject(typeof(UserCompetitionRewardScheme), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
