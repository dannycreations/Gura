using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ObjectModel
{
	public class JsonConfigContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
			List<JsonProperty> list2 = new List<JsonProperty>();
			foreach (JsonProperty jsonProperty in list)
			{
				PropertyInfo property = type.GetProperty(jsonProperty.PropertyName, BindingFlags.Instance | BindingFlags.Public);
				if (property != null)
				{
					foreach (object obj in property.GetCustomAttributes(true))
					{
						if (obj is JsonConfigAttribute)
						{
							list2.Add(jsonProperty);
						}
					}
				}
			}
			return list2;
		}
	}
}
