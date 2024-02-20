using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterUserCompetitionEquipmentAllowed : ArrayOfEnumsToStringConverterBase
	{
		protected override object GetFromReader(JsonReader reader)
		{
			List<UserCompetitionEquipmentAllowed> list = new List<UserCompetitionEquipmentAllowed>();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((UserCompetitionEquipmentAllowed)Enum.Parse(typeof(UserCompetitionEquipmentAllowed), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((UserCompetitionEquipmentAllowed)Enum.ToObject(typeof(UserCompetitionEquipmentAllowed), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
