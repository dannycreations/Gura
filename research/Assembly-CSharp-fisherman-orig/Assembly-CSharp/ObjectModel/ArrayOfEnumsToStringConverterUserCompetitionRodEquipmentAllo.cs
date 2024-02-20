using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionRodEquipmentAllowed : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionRodEquipmentAllowed> list = new List<UserCompetitionRodEquipmentAllowed>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionRodEquipmentAllowed)Enum.Parse(typeof(UserCompetitionRodEquipmentAllowed), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionRodEquipmentAllowed)Enum.ToObject(typeof(UserCompetitionRodEquipmentAllowed), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
